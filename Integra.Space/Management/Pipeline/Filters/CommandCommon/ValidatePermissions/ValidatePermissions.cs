//-----------------------------------------------------------------------
// <copyright file="ValidatePermissions.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Common;
    using Database;
    using Language;
    using Ninject;

    /// <summary>
    /// Filter lock class.
    /// </summary>
    internal class ValidatePermissions : CommandFilter
    {
        /// <inheritdoc />
        public override PipelineContext Execute(PipelineContext context)
        {
            // acción del comando.
            ActionCommandEnum action = context.CommandContext.Command.Action;

            // obtengo el contexto de la base de datos.
            SpaceDbContext databaseContext = context.Kernel.Get<SpaceDbContext>();

            Login login = context.SecurityContext.Login;

            // validar si pertenece a un role del servidor
            ServerRoleValidator serverRoleValidator = new ServerRoleValidator(databaseContext, login, action);
            if (serverRoleValidator.BelongsToServerRole())
            {
                return context;
            }

            // estructura para validar los owners
            HashSet<CommandObject> objects = context.CommandContext.Command.CommandObjects;
            IEnumerable<ViewPermission> userPermissions = databaseContext.VWPermissions.Where(x => x.PrincipalId == login.LoginId
                                                            && x.ServerIdOfPrincipal == login.ServerId); // Enumerable.Empty<ViewPermission>();

            // obtengo los permisos del usuario y de los roles de base de datos a los que pertenece
            foreach (CommandObject @object in objects)
            {
                if (@object.GranularPermission == PermissionsEnum.None)
                {
                    continue;
                }

                // se toma el usuario del objeto que esta ejecutando el comando.
                DatabaseUser user = @object.GetUser(databaseContext, login);

                // se obtienen los permisos del usuario
                userPermissions = userPermissions.Concat(databaseContext.VWPermissions.Where(x => x.PrincipalId == user.DbUsrId
                                                            && x.DatabaseIdOfPrincipal == user.DatabaseId
                                                            && x.ServerIdOfPrincipal == user.ServerId));

                // se agregan los permisos del login del usuario.
                userPermissions = userPermissions.Concat(databaseContext.VWPermissions.Where(x => x.PrincipalId == login.LoginId && x.ServerIdOfPrincipal == login.ServerId));

                // obtengo los roles de base de datos del usuario.
                context.SecurityContext.DatabaseRoles = databaseContext.DatabaseUsers
                    .Single(x => x.ServerId == user.ServerId && x.DatabaseId == user.DatabaseId && x.DbUsrId == user.DbUsrId)
                    .DatabaseRoles;

                // obtengo los permisos de cada rol de base de datos del usuario y los agrego a la lista de permisos del usuario.
                foreach (DatabaseRole databaseRole in context.SecurityContext.DatabaseRoles)
                {
                    IQueryable<ViewPermission> databaseRolePermissions = databaseContext.VWPermissions.Where(x => x.PrincipalId == databaseRole.DbRoleId
                                                            && x.DatabaseIdOfPrincipal == databaseRole.DatabaseId
                                                            && x.ServerIdOfPrincipal == databaseRole.ServerId);

                    userPermissions = userPermissions.Concat(databaseRolePermissions);
                }
            }

            // si el comando es grant, deny o revoke se obtienen los objetos de los permisos
            if (action == ActionCommandEnum.Grant || action == ActionCommandEnum.Deny || action == ActionCommandEnum.Revoke)
            {
                // elimino los principals del hashset porque sobre ellos no es necesario tener permisos para asignarles permisos.
                objects.RemoveWhere(x => x.SecurableClass == SystemObjectEnum.Login || x.SecurableClass == SystemObjectEnum.DatabaseUser || x.SecurableClass == SystemObjectEnum.DatabaseRole);

                PermissionNode permission = ((PermissionsCommandNode)context.CommandContext.Command).Permission;
                if (permission.CommandObject == null)
                {
                    // se utiliza single porque no tienen que existir permisos repetidos de este tipo.
                    string securableClassName = databaseContext.GranularPermissions
                    .Single(gp => gp.GranularPermissionName.Replace(" ", string.Empty).Equals(permission.Permission.ToString(), StringComparison.InvariantCultureIgnoreCase))
                    .PermissionsBySecurables
                    .Single()
                    .SecurableClass
                    .SecurableName;

                    SystemObjectEnum objectType;
                    if (!Enum.TryParse(securableClassName, true, out objectType))
                    {
                        throw new Exception("Securable class not defined.");
                    }

                    // si es un permiso donde no se especifica el objeto se debe tomar del contexto del comando.
                    switch (objectType)
                    {
                        case SystemObjectEnum.Database:
                            objects.Add(new CommandObject(objectType, login.Database.DatabaseName, PermissionsEnum.Control, false));
                            break;
                        case SystemObjectEnum.Server:
                            objects.Add(new CommandObject(objectType, login.Server.ServerName, PermissionsEnum.ControlServer, false));
                            break;
                        default:
                            throw new Exception(string.Format("System object not allowed for permission {0}.", permission.Permission.ToString()));
                    }
                }
            }

            // el orden es importante, primero granular permissions y luego securable classes
            List<PermissionBySecurable> permissionsBySecurables = databaseContext.PermissionsBySecurables
                .Join<PermissionBySecurable, GranularPermission, Guid, PermissionBySecurable>(databaseContext.GranularPermissions, x => x.GranularPermissionId, x => x.GranularPermissionId, (x, y) => x)
                .Join<PermissionBySecurable, SecurableClass, Guid, PermissionBySecurable>(databaseContext.SecurableClasses, x => x.SecurableClassId, x => x.SecurableClassId, (x, y) => x)
                .ToList();

            OwnerValidator ownerValidator = null;
            SecurableClass secureClass = null;
            bool isOwnerOfAll = true;
            PermissionBySecurable pbs = null;
            HashSet<CommandObject> objetosDeLosQueEsOwner = new HashSet<CommandObject>();
            foreach (CommandObject @object in objects)
            {
                if (@object.GranularPermission == PermissionsEnum.None)
                {
                    continue;
                }

                Schema schema = @object.GetSchema(databaseContext, login);

                if (secureClass == null || !secureClass.SecurableName.Equals(@object.SecurableClass.ToString(), StringComparison.InvariantCultureIgnoreCase))
                {
                    IEnumerable<PermissionBySecurable> pbsListAux = permissionsBySecurables.Where(x => x.GranularPermission.GranularPermissionName.Replace(" ", string.Empty).Equals(@object.GranularPermission.ToString(), StringComparison.InvariantCultureIgnoreCase));

                    // si la lista contiene un elemento se toma ese y continua, de lo contrario toma el registro que coincida con el tipo de objeto, si hay mas de uno es error.
                    if (pbsListAux.Count() == 1)
                    {
                        pbs = pbsListAux.First();
                    }
                    else if (pbsListAux.Count() > 1)
                    {
                        pbs = pbsListAux.Single(x => x.SecurableClass.SecurableName.Equals(@object.SecurableClass.ToString(), StringComparison.InvariantCultureIgnoreCase));
                    }
                    else
                    {
                        throw new Exception("Invalid permission.");
                    }

                    // se obtienen la clase de objeto asegurable.
                    secureClass = pbs.SecurableClass;
                }

                // se toma el usuario del objeto que esta ejecutando el comando.
                DatabaseUser user = @object.GetUser(databaseContext, login);

                if (action == ActionCommandEnum.Create)
                {
                    ownerValidator = new OwnerValidator(context.SecurityContext.Login, user, schema, secureClass, databaseContext, null);
                }
                else
                {
                    ownerValidator = new OwnerValidator(context.SecurityContext.Login, user, schema, secureClass, databaseContext, @object.Name);
                }

                if (!ownerValidator.IsOwner())
                {
                    // evita hacer un if, cuando ya sea false no modificar su valor de regreso a true
                    // no poner un break aqui porque se necesita ir llenando la lista con los objetos de los que si es owner para luego quitarlos de la lista.
                    isOwnerOfAll = isOwnerOfAll && false;
                }
                else
                {
                    objetosDeLosQueEsOwner.Add(@object);
                }
            }

            // si es owner de todos los objetos del contexto de ejecución del comando se continua con la ejecución del pipeline
            if (isOwnerOfAll)
            {
                return context;
            }

            // remuevo los objetos del arreglo del cual el usuario es owner.
            // abajo se validará para los comandos grant, revoke y deny que el usuario tenga el permiso "control" sobre los objetos restantes.
            objects.RemoveWhere(x => objetosDeLosQueEsOwner.Contains(x, new CommandObjectComparer()));

            // obtengo los permisos del contexto de ejecución para el usuario especificado
            IQueryable<ViewPermission> userPermissionsOfTheContext = null;
            this.GetContextPermissions(action, databaseContext, login, objects, userPermissions, out userPermissionsOfTheContext);

            // si no tiene ningun permiso sobre el contexto de ejecución se lanza una excepción.
            if (userPermissionsOfTheContext.Count() == 0)
            {
                throw new Exception("Invalid permissions.");
            }

            GranularPermission granularPermission = null;
            SecurableClass securableClass = null;
            PermissionBySecurable permissionBySecurable = null;
            bool hasPermissions = false;
            foreach (CommandObject @object in objects)
            {
                if (@object.GranularPermission == PermissionsEnum.None)
                {
                    continue;
                }

                if (securableClass == null || !securableClass.SecurableName.Equals(@object.SecurableClass.ToString(), StringComparison.InvariantCultureIgnoreCase))
                {
                    IEnumerable<PermissionBySecurable> pbsListAux = permissionsBySecurables.Where(x => x.GranularPermission.GranularPermissionName.Replace(" ", string.Empty).Equals(@object.GranularPermission.ToString(), StringComparison.InvariantCultureIgnoreCase));

                    // si la lista contiene un elemento se toma ese y continua, de lo contrario toma el registro que coincida con el tipo de objeto, si hay mas de uno es error.
                    if (pbsListAux.Count() == 1)
                    {
                        permissionBySecurable = pbsListAux.First();
                    }
                    else
                    {
                        permissionBySecurable = pbsListAux.Single(x => x.SecurableClass.SecurableName.Equals(@object.SecurableClass.ToString(), StringComparison.InvariantCultureIgnoreCase));
                    }

                    granularPermission = permissionBySecurable.GranularPermission;

                    // se obtienen la clase de objeto asegurable.
                    securableClass = permissionBySecurable.SecurableClass;
                }

                // se obtienen la jerarquía de permisos en base al tipo de objeto y al permiso granular.
                HashSet<string> hashSetParentPermissions = new HashSet<string>();
                IEnumerable<CommandPermission> requiredPermissions = databaseContext.Database.SqlQuery<CommandPermission>("[space].[get_permissions_with_parents] @granularPermissionName = {0}, @secureClassName = {1}", granularPermission.GranularPermissionName, securableClass.SecurableName);

                if (requiredPermissions.Count() == 0)
                {
                    throw new Exception("Invalid permissions.");
                }

                // se obtiene el permiso mas especifico necesario, es decir, el de nivel mas bajo, en el arbol de permisos, necesario para ejecutar el comando.          
                ViewPermission viewPermission = new ViewPermission();
                viewPermission.GranularPermissionId = requiredPermissions.First().ChildGPId; // granularPermission.GranularPermissionId;
                viewPermission.SecurableClassId = requiredPermissions.First().ChildSCId; // securableClass.SecurableClassId;

                // se crea la lista de permisos necesarios para ejecutar el comando. El usuario debe tener por lo menos uno de ellos para poder ejecutar el comando.
                List<ViewPermission> listOfPermissions = new List<ViewPermission>();

                // se agrega el permiso mas especifico a la lista
                listOfPermissions.Add(viewPermission);

                foreach (CommandPermission requieredPermission in requiredPermissions)
                {
                    string[] parentPermissions = requieredPermission.Parents.Split(',');
                    foreach (string parentPermission in parentPermissions)
                    {
                        hashSetParentPermissions.Add(parentPermission);
                    }
                }

                // se obtienen todos los identificadores de los permisos padres.
                // (mas adelante podría retornarse un json desde el sp aunque puede hacer que el performance se reduzca por la deserialización).
                foreach (string parentPermission in hashSetParentPermissions)
                {
                    viewPermission = new ViewPermission();

                    // se especifica el permiso.
                    string[] permissionAux = parentPermission.Split(' ');
                    viewPermission.GranularPermissionId = Guid.Parse(permissionAux[0]);
                    viewPermission.SecurableClassId = Guid.Parse(permissionAux[1]);

                    listOfPermissions.Add(viewPermission);
                }

                // se hace la correlación entre los permisos que tiene el usuario y los permisos requeridos para ejecutar el comando.
                Func<ViewPermission, dynamic> funcKeySelector = x => new { x.GranularPermissionId, x.SecurableClassId };
                IEnumerable<ViewPermission> permissionsToExecuteCommand = userPermissionsOfTheContext.Join(listOfPermissions, funcKeySelector, funcKeySelector, (x, y) => x);

                /* se hace una operacion OR entre todos los niveles del arbol de permisos resultante
                 luego se hace una operación XOR entre los valores de los campos granted y denied resultantes de la operación OR
                 si el resultado de la operacion XOR es 1: continue, de lo contrario lance error. */

                bool granted = false;
                bool denied = false;
                foreach (ViewPermission permissionToExecuteCommand in permissionsToExecuteCommand)
                {
                    if (!granted)
                    {
                        // la autorizacion será igual "granted" y "withGrantOption" siempre y cuando el comando sea grant y el permiso definido 
                        // en el objeto es igual al permiso de esta iteración, es decir, el permiso mas específico.
                        if (action == ActionCommandEnum.Grant)
                        {
                            if (granularPermission.GranularPermissionId == permissionToExecuteCommand.GranularPermissionId)
                            {
                                granted = permissionToExecuteCommand.Granted && permissionToExecuteCommand.WithGrantOption;
                            }
                        }

                        granted = permissionToExecuteCommand.Granted;
                    }

                    if (!denied)
                    {
                        denied = permissionToExecuteCommand.Denied;
                    }

                    // como es una operación OR entre los niveles del arbol, una vez que los dos, granted y denied, son true se sale del foreach porque no vale la pena continuar evaluando.
                    if (granted && denied)
                    {
                        break;
                    }
                }

                // si tiene permisos se continua la ejecución del pipeline, de lo contrario se lanza un error.
                if (granted == true && denied == false)
                {
                    hasPermissions = true;
                }
                else
                {
                    hasPermissions = false;
                    break;
                }
            }

            if (hasPermissions)
            {
                return context;
            }
            else
            {
                throw new Exception("Invalid permissions.");
            }
        }

        /// <inheritdoc />
        public override void OnError(PipelineContext e)
        {
        }

        /// <summary>
        /// Gets the context permissions of the specified user.
        /// </summary>
        /// <param name="action">Command action.</param>
        /// <param name="databaseContext">Database context</param>
        /// <param name="login">Login of the user.</param>
        /// <param name="objects">Objects of the command.</param>
        /// <param name="userPermissions">User permissions.</param>
        /// <param name="userPermissionsOfTheContext">User context permissions.</param>
        private void GetContextPermissions(ActionCommandEnum action, SpaceDbContext databaseContext, Login login, HashSet<CommandObject> objects, IEnumerable<ViewPermission> userPermissions, out IQueryable<ViewPermission> userPermissionsOfTheContext)
        {
            userPermissionsOfTheContext = Enumerable.Empty<ViewPermission>().AsQueryable();
            List<ViewPermission> userPermissionsList = userPermissions.ToList();
            Schema schemaOld = null;

            foreach (CommandObject @object in objects)
            {
                Schema schema = @object.GetSchema(databaseContext, login);

                // si el esquema actual es igual al anterior no se agregan los permisos del contexto del objeto para no tener repetidos.
                if (!(schemaOld != null && schemaOld.ServerId == schema.ServerId && schemaOld.DatabaseId == schema.DatabaseId && schemaOld.SchemaId == schema.SchemaId))
                {
                    userPermissionsOfTheContext = userPermissionsOfTheContext.Concat(userPermissionsList.Where(x =>
                                               (x.ServerIdOfSecurable == null && x.DatabaseIdOfSecurable == null && x.SchemaIdOfSecurable == null && x.SecurableId == schema.ServerId) // para servers
                                            || (x.ServerIdOfSecurable == schema.ServerId && x.DatabaseIdOfSecurable == null && x.SchemaIdOfSecurable == null && x.SecurableId == schema.DatabaseId) // base de datos
                                            || (x.ServerIdOfSecurable == schema.ServerId && x.DatabaseIdOfSecurable == schema.DatabaseId && x.SchemaIdOfSecurable == null && x.SecurableId == schema.SchemaId))); // para esquemas
                }

                // si se va a crear un nuevo objeto se termina el método. No se obtienen los permisos de ese objeto porque no existe.
                if (@object.IsNew)
                {
                    continue;
                }

                schemaOld = schema;

                Guid entityId;
                switch (@object.SecurableClass)
                {
                    case SystemObjectEnum.Server:
                        entityId = databaseContext.Servers.Single(x => x.ServerName == @object.Name).ServerId;
                        userPermissionsOfTheContext = userPermissionsOfTheContext.Concat(userPermissionsList.Where(x => x.ServerIdOfSecurable == null && x.DatabaseIdOfSecurable == null && x.SchemaIdOfSecurable == null && x.SecurableId == entityId));
                        break;
                    case SystemObjectEnum.Endpoint:
                        entityId = databaseContext.Endpoints.Single(x => x.ServerId == schema.ServerId && x.EnpointName == @object.Name).EndpointId;
                        userPermissionsOfTheContext = userPermissionsOfTheContext.Concat(userPermissionsList.Where(x => x.ServerIdOfSecurable == schema.ServerId && x.DatabaseIdOfSecurable == null && x.SchemaIdOfSecurable == null && x.SecurableId == entityId));
                        break;
                    case SystemObjectEnum.ServerRole:
                        entityId = databaseContext.ServerRoles.Single(x => x.ServerId == schema.ServerId && x.ServerRoleName == @object.Name).ServerRoleId;
                        userPermissionsOfTheContext = userPermissionsOfTheContext.Concat(userPermissionsList.Where(x => x.ServerIdOfSecurable == schema.ServerId && x.DatabaseIdOfSecurable == null && x.SchemaIdOfSecurable == null && x.SecurableId == entityId));
                        break;
                    case SystemObjectEnum.Login:
                        entityId = databaseContext.Logins.Single(x => x.ServerId == schema.ServerId && x.LoginName == @object.Name).LoginId;
                        userPermissionsOfTheContext = userPermissionsOfTheContext.Concat(userPermissionsList.Where(x => x.ServerIdOfSecurable == schema.ServerId && x.DatabaseIdOfSecurable == null && x.SchemaIdOfSecurable == null && x.SecurableId == entityId));
                        break;
                    case SystemObjectEnum.Database:
                        entityId = databaseContext.Databases.Single(x => x.ServerId == schema.ServerId && x.DatabaseName == @object.Name).DatabaseId;
                        userPermissionsOfTheContext = userPermissionsOfTheContext.Concat(userPermissionsList.Where(x => x.ServerIdOfSecurable == schema.ServerId && x.DatabaseIdOfSecurable == null && x.SchemaIdOfSecurable == null && x.SecurableId == entityId));
                        break;
                    case SystemObjectEnum.DatabaseRole:
                        entityId = databaseContext.DatabaseRoles.Single(x => x.ServerId == schema.ServerId && x.DatabaseId == schema.DatabaseId && x.DbRoleName == @object.Name).DbRoleId;
                        userPermissionsOfTheContext = userPermissionsOfTheContext.Concat(userPermissionsList.Where(x => x.ServerIdOfSecurable == schema.ServerId && x.DatabaseIdOfSecurable == schema.DatabaseId && x.SchemaIdOfSecurable == null && x.SecurableId == entityId));
                        break;
                    case SystemObjectEnum.DatabaseUser:
                        entityId = databaseContext.DatabaseUsers.Single(x => x.ServerId == schema.ServerId && x.DatabaseId == schema.DatabaseId && x.DbUsrName == @object.Name).DbUsrId;
                        userPermissionsOfTheContext = userPermissionsOfTheContext.Concat(userPermissionsList.Where(x => x.ServerIdOfSecurable == schema.ServerId && x.DatabaseIdOfSecurable == schema.DatabaseId && x.SchemaIdOfSecurable == null && x.SecurableId == entityId));
                        break;
                    case SystemObjectEnum.Schema:
                        entityId = databaseContext.Schemas.Single(x => x.ServerId == schema.ServerId && x.DatabaseId == schema.DatabaseId && x.SchemaName == @object.Name).SchemaId;
                        userPermissionsOfTheContext = userPermissionsOfTheContext.Concat(userPermissionsList.Where(x => x.ServerIdOfSecurable == schema.ServerId && x.DatabaseIdOfSecurable == schema.DatabaseId && x.SchemaIdOfSecurable == null && x.SecurableId == entityId));
                        break;
                    case SystemObjectEnum.Source:
                        entityId = databaseContext.Sources.Single(x => x.ServerId == schema.ServerId && x.DatabaseId == schema.DatabaseId && x.SchemaId == schema.SchemaId && x.SourceName == @object.Name).SourceId;
                        userPermissionsOfTheContext = userPermissionsOfTheContext.Concat(userPermissionsList.Where(x => x.ServerIdOfSecurable == schema.ServerId && x.DatabaseIdOfSecurable == schema.DatabaseId && x.SchemaIdOfSecurable == schema.SchemaId && x.SecurableId == entityId));
                        break;
                    case SystemObjectEnum.Stream:
                        entityId = databaseContext.Streams.Single(x => x.ServerId == schema.ServerId && x.DatabaseId == schema.DatabaseId && x.SchemaId == schema.SchemaId && x.StreamName == @object.Name).StreamId;
                        userPermissionsOfTheContext = userPermissionsOfTheContext.Concat(userPermissionsList.Where(x => x.ServerIdOfSecurable == schema.ServerId && x.DatabaseIdOfSecurable == schema.DatabaseId && x.SchemaIdOfSecurable == schema.SchemaId && x.SecurableId == entityId));
                        break;
                    case SystemObjectEnum.View:
                        entityId = databaseContext.Views.Single(x => x.ServerId == schema.ServerId && x.DatabaseId == schema.DatabaseId && x.SchemaId == schema.SchemaId && x.ViewName == @object.Name).ViewId;
                        userPermissionsOfTheContext = userPermissionsOfTheContext.Concat(userPermissionsList.Where(x => x.ServerIdOfSecurable == schema.ServerId && x.DatabaseIdOfSecurable == schema.DatabaseId && x.SchemaIdOfSecurable == schema.SchemaId && x.SecurableId == entityId));
                        break;
                    default:
                        throw new Exception("No space object supported.");
                }
            }
        }

        /// <summary>
        /// Command permission class.
        /// </summary>
        private class CommandPermission
        {
            /// <summary>
            /// Gets or sets the property.
            /// </summary>
            public System.Guid ParentGPId { get; set; }

            /// <summary>
            /// Gets or sets the property.
            /// </summary>
            public System.Guid ParentSCId { get; set; }

            /// <summary>
            /// Gets or sets the property.
            /// </summary>
            public System.Guid ChildGPId { get; set; }

            /// <summary>
            /// Gets or sets the property.
            /// </summary>
            public System.Guid ChildSCId { get; set; }

            /// <summary>
            /// Gets or sets the property.
            /// </summary>
            public string Parents { get; set; }
        }
    }
}