//-----------------------------------------------------------------------
// <copyright file="MetadataQueryParserFilter.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Reflection.Emit;
    using Common;
    using Database;
    using Language;
    using Language.Runtime;
    using Ninject;

    /// <summary>
    /// Create command action class.
    /// </summary>
    /// <typeparam name="TIn">System object type.</typeparam>
    internal abstract class MetadataQueryParserFilter<TIn> : CommandFilter where TIn : class
    {
        /// <summary>
        /// System object type.
        /// </summary>
        private SystemObjectEnum objectType;

        /// <summary>
        /// Property name that returns the object name.
        /// </summary>
        private string propertyName;

        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataQueryParserFilter{TIn}"/> class.
        /// </summary>
        /// <param name="objectType">System object type.</param>
        /// <param name="propertyName">Property name that returns the object name.</param>
        public MetadataQueryParserFilter(SystemObjectEnum objectType, string propertyName)
        {
            this.objectType = objectType;
            this.propertyName = propertyName;
        }

        /// <inheritdoc />
        public override PipelineContext Execute(PipelineContext context)
        {
            QueryCommandForMetadataNode command = (QueryCommandForMetadataNode)context.CommandContext.Command;
            Schema schema = context.CommandContext.Schema;

            bool printLog = false;
            string streamName = string.Empty;
            ManagementSchedulerFactory dsf = new ManagementSchedulerFactory();
            bool debugMode = false;
            bool measureElapsedTime = false;
            bool isTestMode = false;
            CompileContext compileContext = new CompileContext()
            {
                PrintLog = printLog,
                QueryName = streamName,
                Scheduler = dsf,
                DebugMode = debugMode,
                MeasureElapsedTime = measureElapsedTime,
                IsTestMode = isTestMode
            };

            SpaceAssemblyBuilder sasmBuilder = new SpaceAssemblyBuilder("SpaceQueryAssembly_" + compileContext.QueryName);
            AssemblyBuilder asmBuilder = sasmBuilder.CreateAssemblyBuilder();
            SpaceModuleBuilder modBuilder = new SpaceModuleBuilder(asmBuilder);
            modBuilder.CreateModuleBuilder();

            TreeTransformations tf = new TreeTransformations(asmBuilder, command.ExecutionPlan);
            tf.Transform();

            compileContext.AsmBuilder = asmBuilder;
            CodeGenerator cg = new CodeGenerator(compileContext);
            Delegate resultFunc = cg.CompileDelegate(command.ExecutionPlan);
            IObservable<TIn> input = this.GetMetadata(context);
            IObservable<object> finalResultMetadata = null;

            if (isTestMode)
            {
                finalResultMetadata = (IObservable<object>)resultFunc.DynamicInvoke(input, System.Reactive.Concurrency.ThreadPoolScheduler.Instance);
            }
            else
            {
                finalResultMetadata = (IObservable<object>)resultFunc.DynamicInvoke(input);
            }

            /*input
                .ObserveOn(System.Reactive.Concurrency.ThreadPoolScheduler.Instance)
                .Subscribe(x =>
                {
                    Console.WriteLine(x);
                });

            input
                .ObserveOn(System.Reactive.Concurrency.ThreadPoolScheduler.Instance)
                .Subscribe(x =>
                {
                    Console.WriteLine(x);
                });*/
            
            List<dynamic> result = new List<dynamic>();
            finalResultMetadata
                .Publish().RefCount()
                /*.ObserveOn(System.Reactive.Concurrency.ThreadPoolScheduler.Instance)*/
                /*.SubscribeOn(System.Reactive.Concurrency.ThreadPoolScheduler.Instance)*/
                .Subscribe(x =>
                {
                    var test1 = ((Array)x.GetType().GetProperty("Result").GetValue(x)).GetValue(0).GetType().GetProperty("servId");
                    var test2 = ((Array)x.GetType().GetProperty("Result").GetValue(x)).GetValue(0).GetType().GetProperty("maxTest");
                    var xxxx = (object)(new
                    {
                        servId = (Guid)((Array)x.GetType().GetProperty("Result").GetValue(x)).GetValue(0).GetType().GetProperty("servId").GetValue(((Array)x.GetType().GetProperty("Result").GetValue(x)).GetValue(0)),
                        abc = (int)((Array)x.GetType().GetProperty("Result").GetValue(x)).GetValue(0).GetType().GetProperty("maxTest").GetValue(((Array)x.GetType().GetProperty("Result").GetValue(x)).GetValue(0))
                    });
                    result.Add(xxxx);
                });

            return context;
        }

        /// <summary>
        /// Gets the function to select the key of the objects for the join statement between the objects and the permissions.
        /// </summary>
        /// <returns>The function to select the key of the objects for the join statement between the objects and the permissions.</returns>
        protected abstract Func<TIn, dynamic> GetObjectKeySelector();

        /// <summary>
        /// Gets the function to select the key of the permissions for the join statement between the objects and the permissions.
        /// </summary>
        /// <returns>The function to select the key of the permissions for the join statement between the objects and the permissions.</returns>
        protected abstract Func<ViewPermission, dynamic> GetViewPermissionKeySelector();

        /// <summary>
        /// Gets the function to evaluate whether an object exists at the object result set.
        /// </summary>
        /// <param name="object">Object to validate.</param>
        /// <returns>The function to evaluate whether an object exists at the object result set.</returns>
        protected abstract Func<TIn, bool> GetPredicateForExtensionAny(TIn @object);

        /// <summary>
        /// Gets the database set for the actual filter.
        /// </summary>
        /// <param name="context">Database context.</param>
        /// <returns>The database set for the actual filter.</returns>
        protected abstract DbSet<TIn> GetDbSet(SpaceDbContext context);

        /// <summary>
        /// Get the permissions of the actual user.
        /// </summary>
        /// <param name="securityContext">Security context.</param>
        /// <param name="databaseContext">Database context.</param>
        /// <param name="login">Login of the user.</param>
        /// <param name="user">Logged user.</param>
        /// <param name="securableClass">Securable system object class.</param>
        /// <param name="objects">System objects.</param>
        /// <returns>Set of permissions.</returns>
        protected IEnumerable<ViewPermission> GetPermissions(SecurityContext securityContext, SpaceDbContext databaseContext, Login login, DatabaseUser user, SecurableClass securableClass, DbSet<TIn> objects)
        {
            // se obtienen los permisos del usuario
            IQueryable<ViewPermission> userPermissions = databaseContext.VWPermissions.Where(x => x.PrincipalId == user.DbUsrId
                                                        && x.DatabaseIdOfPrincipal == user.DatabaseId
                                                        && x.ServerIdOfPrincipal == user.ServerId
                                                        && x.SecurableClassId == securableClass.SecurableClassId);

            // se agregan los permisos del login del usuario.
            userPermissions = userPermissions.Concat(databaseContext.VWPermissions.Where(x => x.PrincipalId == login.LoginId && x.ServerIdOfPrincipal == login.ServerId && x.SecurableClassId == securableClass.SecurableClassId));

            // obtengo los roles de base de datos del usuario.
            securityContext.DatabaseRoles = databaseContext.DatabaseUsers
                .Single(x => x.ServerId == user.ServerId && x.DatabaseId == user.DatabaseId && x.DbUsrId == user.DbUsrId)
                .DatabaseRoles;

            // obtengo los permisos de cada rol de base de datos del usuario y los agrego a la lista de permisos del usuario.
            foreach (DatabaseRole databaseRole in securityContext.DatabaseRoles)
            {
                IQueryable<ViewPermission> databaseRolePermissions = databaseContext.VWPermissions.Where(x => x.PrincipalId == databaseRole.DbRoleId
                                                        && x.DatabaseIdOfPrincipal == databaseRole.DatabaseId
                                                        && x.ServerIdOfPrincipal == databaseRole.ServerId
                                                        && x.SecurableClassId == securableClass.SecurableClassId);

                userPermissions = userPermissions.Concat(databaseRolePermissions);
            }

            // se obtienen los permisos necesarios para ejecutar el comando.
            GranularPermission granularPermission = databaseContext.GranularPermissions.Single(x => x.GranularPermissionName.Replace(" ", string.Empty).Equals(PermissionsEnum.ViewDefinition.ToString(), StringComparison.InvariantCultureIgnoreCase));
            CommandPermission requiredPermissions = databaseContext.Database.SqlQuery<CommandPermission>("[space].[get_permissions_with_parents] @granularPermissionName = {0}, @secureClassName = {1}", granularPermission.GranularPermissionName, securableClass.SecurableName).Single();
            string[] parentPermissions = requiredPermissions.Parents.Split(',');

            // se obtiene el permiso mas especifico necesario, es decir, el de nivel mas bajo, en el arbol de permisos, necesario para ejecutar el comando.          
            ViewPermission viewPermission = new ViewPermission();
            viewPermission.GranularPermissionId = requiredPermissions.ChildGPId;
            viewPermission.SecurableClassId = requiredPermissions.ChildSCId;

            // se crea la lista de permisos necesarios para ejecutar el comando. El usuario debe tener por lo menos uno de ellos para poder ejecutar el comando.
            List<ViewPermission> listOfPermissions = new List<ViewPermission>();

            // se agrega el permiso mas especifico a la lista
            listOfPermissions.Add(viewPermission);

            // se obtienen todos los identificadores de los permisos padres.
            // (mas adelante podría retornarse un json desde el sp aunque puede hacer que el performance se reduzca por la deserialización).
            foreach (string parentPermission in parentPermissions)
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
            IEnumerable<ViewPermission> permissionsToExecuteCommand = userPermissions.Join(listOfPermissions, funcKeySelector, funcKeySelector, (x, y) => x);
            return permissionsToExecuteCommand;
        }

        /// <summary>
        /// Gets the objects that you own.
        /// </summary>
        /// <param name="login">Login of the user.</param>
        /// <param name="user">Database user.</param>
        /// <param name="schema">Execution schema.</param>
        /// <param name="securableClass">Securable class.</param>
        /// <param name="databaseContext">Database context.</param>
        /// <param name="objects">Full set of objects.</param>
        /// <returns>The set of objects that you own.</returns>
        protected IEnumerable<TIn> GetOwnedObjects(Login login, DatabaseUser user, Schema schema, SecurableClass securableClass, SpaceDbContext databaseContext, IEnumerable<TIn> objects)
        {
            List<TIn> ownedObjects = new List<TIn>();
            OwnerValidator ownerValidator = null;
            foreach (TIn @object in objects)
            {
                string objectName = @object.GetType().GetProperty(this.propertyName).GetValue(@object).ToString();
                ownerValidator = new OwnerValidator(login, user, schema, securableClass, databaseContext, objectName);

                if (ownerValidator.IsOwner())
                {
                    ownedObjects.Add(@object);
                }
            }

            return ownedObjects;
        }

        /// <summary>
        /// Compiles the execution plan.
        /// </summary>
        /// <param name="context">Pipeline context.</param>
        /// <returns>The compiled function.</returns>
        private IObservable<TIn> GetMetadata(PipelineContext context)
        {
            // acción del comando.
            ActionCommandEnum action = context.CommandContext.Command.Action;

            // se toma el usuario que esta ejecutando el comando.
            DatabaseUser user = context.SecurityContext.User;

            // se obtiene el esquema de ejecución.
            Space.Database.Schema schema = context.CommandContext.Schema;

            // obtengo el contexto de la base de datos.
            SpaceDbContext databaseContext = context.Kernel.Get<SpaceDbContext>();

            Login login = context.SecurityContext.Login;

            SecurableClass securableClass = databaseContext.SecurableClasses.Single(x => x.SecurableName.Equals(this.objectType.ToString(), StringComparison.InvariantCultureIgnoreCase));

            DbSet<TIn> fullSetOfObjects = this.GetDbSet(databaseContext);

            // validar si pertenece a un role del servidor
            ServerRoleValidator serverRoleValidator = new ServerRoleValidator(databaseContext, login, action);
            if (serverRoleValidator.BelongsToServerRole())
            {
                return fullSetOfObjects.ToObservable();
            }
            else
            {
                IEnumerable<ViewPermission> permissions = this.GetPermissions(context.SecurityContext, databaseContext, login, user, securableClass, fullSetOfObjects);
                Func<TIn, dynamic> keySelector1 = this.GetObjectKeySelector();
                Func<ViewPermission, dynamic> keySelector2 = this.GetViewPermissionKeySelector();
                List<TIn> objects = fullSetOfObjects.Join(permissions, keySelector1, keySelector2, (x, y) => x).ToList();

                IEnumerable<TIn> ownedObject = this.GetOwnedObjects(login, user, schema, securableClass, databaseContext, objects);
                foreach (TIn ownedStream in ownedObject)
                {
                    if (!objects.Any(this.GetPredicateForExtensionAny(ownedStream)))
                    {
                        objects.Add(ownedStream);
                    }
                }

                return objects
                    .ToObservable(/*System.Reactive.Concurrency.ThreadPoolScheduler.Instance*/)
                    .Publish()
                    .RefCount();
                    /*.ToObservable(System.Reactive.Concurrency.ThreadPoolScheduler.Instance)*/
                    /*.Replay(System.Reactive.Concurrency.ThreadPoolScheduler.Instance);*/                    
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