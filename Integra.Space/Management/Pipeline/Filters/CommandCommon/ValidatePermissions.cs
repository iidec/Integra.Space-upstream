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
    using System.Reflection;
    using Common;
    using Models;
    using Ninject;
    using Repos;

    /// <summary>
    /// Filter lock class.
    /// </summary>
    /// <typeparam name="TPermission">Permission type.</typeparam>
    internal abstract class ValidatePermissions<TPermission> : CommandFilter where TPermission : PermissionAssigned
    {
        /// <inheritdoc />
        public override PipelineContext Execute(PipelineContext context)
        {
            // acción del comando.
            ActionCommandEnum action = context.Command.Action;

            // se toma el usuario que esta ejecutando el comando.
            User user = context.User;

            // se obtiene el esquema de ejecución
            Schema schema = this.GetSchema(context);

            SystemRoleCacheRepository systemRoleRepo = (SystemRoleCacheRepository)context.Kernel.Get<SystemRepositoryBase<SystemRole>>();
            bool isSysAdmin = systemRoleRepo.FindByName(SystemRolesEnum.SysAdmin).Users.Contains<User>(user, new UserComparer());

            // si es sysadmin no valide nada mas y continue la ejecución del pipeline
            if (isSysAdmin)
            {
                return context;
            }
            else
            {
                // Valida otros permisos de roles de sistema
                if (action == ActionCommandEnum.Read)
                {
                    // si el comando es read valido si el usuario pertenece al role SysReader.
                    bool isSysReader = systemRoleRepo.FindByName(SystemRolesEnum.SysReader).Users.Contains(user, new UserComparer());
                    if (isSysReader)
                    {
                        return context;
                    }
                    else
                    {
                        throw new Exception("User does not have permissions to execute the command.");
                    }
                }
                else if (action == ActionCommandEnum.Create && context.Command.SpaceObjectType == SystemObjectEnum.Schema)
                {
                    // si el comando es create schema el usuario tiene que pertenecer 
                    bool isSchemaCreator = systemRoleRepo.FindByName(SystemRolesEnum.SchemaCreator).Users.Contains(user, new UserComparer());
                    if (isSchemaCreator)
                    {
                        context.IsAllowedOverSpecificObject = true;
                        context.IsAllowedOverSystem = true;
                        return context;
                    }
                    else
                    {
                        throw new Exception("User does not have permissions to execute the command.");
                    }
                }
                else
                {
                    // si no pertenece a ningun rol del sistema busco si tiene permisos asignados

                    // obtengo los permisos necesarios para ejecutar el comando.
                    IEnumerable<TPermission> commandPermissions = this.GetPermissionsOfTheCommand(user, context.Kernel, context.Command, schema)
                                                                    .Where(x => x is TPermission).Cast<TPermission>();

                    // obtengo los permisos del usuario completos para todo el sistema, haciendo OR entre los especificos del usuario mas los de los roles que tiene asignados.
                    IEnumerable<TPermission> userPermissions = this.GetUserPermissions(user, context.Kernel);

                    bool isAllowed = false;
                    foreach (TPermission commandPermission in commandPermissions)
                    {
                        if (userPermissions.Contains(commandPermission))
                        {
                            isAllowed = true;
                        }
                        else
                        {
                            isAllowed = false;
                            break;
                        }
                    }

                    if (!isAllowed)
                    {
                        throw new Exception("User does not have permissions to execute the command.");
                    }
                    else
                    {
                        return context;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the user permissions in the system.
        /// </summary>
        /// <param name="user">User that want to execute the command.</param>
        /// <param name="kernel">DI kernel.</param>
        /// <returns>The user permissions in the system.</returns>
        public abstract IEnumerable<TPermission> GetUserPermissions(User user, IKernel kernel);

        /// <summary>
        /// Validate whether the user permission.
        /// </summary>
        /// <param name="commandPermissions">Permissions requiring to execute the command.</param>
        /// <param name="userPermissions">Permissions of the user.</param>
        /// <returns>A value indicating whether the user has the required permissions.</returns>
        public abstract bool ValidatePermission(IEnumerable<TPermission> commandPermissions, IEnumerable<TPermission> userPermissions);

        /// <inheritdoc />
        public override void OnError(PipelineContext e)
        {
        }

        /// <summary>
        /// Get the permissions required to execute the actual command.
        /// </summary>
        /// <param name="user">User requesting the command execution.</param>
        /// <param name="kernel">DI kernel.</param>
        /// <param name="command">Command to execute.</param>
        /// <param name="schema">Schema execution of the command.</param>
        /// <returns>List of permissions required to execute the actual command.</returns>
        private List<PermissionAssigned> GetPermissionsOfTheCommand(Principal user, IKernel kernel, Language.SystemCommand command, Schema schema)
        {
            List<PermissionAssigned> requestedPermissions = new List<PermissionAssigned>();
            foreach (Tuple<SystemObjectEnum, string> @object in command.GetUsedSpaceObjects())
            {
                if (string.IsNullOrWhiteSpace(@object.Item2))
                {
                    requestedPermissions.Add(new PermissionOverObjectType(user, @object.Item1, (int)command.PermissionValue, 0, schema));
                }
                else
                {
                    MethodInfo method2 = typeof(ResolutionExtensions).GetMethods()
                    .First(m => m.Name == "Get" && m.GetParameters()[1].ParameterType.Equals(typeof(Ninject.Parameters.IParameter).MakeArrayType()))
                    .MakeGenericMethod(new Type[] { typeof(IRepository<>).MakeGenericType(Type.GetType("Integra.Space.Models." + @object.Item1.ToString())) });

                    var repo = method2.Invoke(null, new object[] { kernel, new Ninject.Parameters.IParameter[] { } });
                    SystemObject systemObject = (SystemObject)repo.GetType().GetMethod("FindByName").Invoke(repo, new object[] { @object.Item2 });
                    requestedPermissions.Add(new PermissionOverSpecificObject(user, systemObject, (int)command.PermissionValue, 0));
                }
            }

            return requestedPermissions;
        }

        /// <summary>
        /// User comparer class.
        /// </summary>
        private class UserComparer : IEqualityComparer<User>
        {
            /// <inheritdoc />
            public bool Equals(User x, User y)
            {
                if (x.Name == y.Name)
                {
                    return true;
                }

                return false;
            }

            /// <inheritdoc />
            public int GetHashCode(User obj)
            {
                return obj.Name.GetHashCode();
            }
        }
    }
}