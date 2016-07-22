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
            // se toma el usuario que esta ejecutando el comando.
            User user = context.User;

            // se obtiene el esquema de ejecución
            Schema schema = this.GetSchema(context);

            IEnumerable<TPermission> commandPermissions = this.GetPermissionsOfTheCommand(user, context.Kernel, context.Command)
                                                                .Where(x => x is TPermission).Cast<TPermission>();

            PermissionCacheRepository<TPermission> permissionRepo = context.Kernel.Get<PermissionCacheRepository<TPermission>>();
            IEnumerable<TPermission> userPermissions = permissionRepo.List.Where(x => x.Principal.Name == user.Name);

            ActionCommandEnum action = context.Command.Action;
            SystemRoleCacheRepository systemRoleRepo = (SystemRoleCacheRepository)context.Kernel.Get<IRepository<SystemRole>>();

            bool isSysAdmin = systemRoleRepo.FindByName(SystemRolesEnum.SysAdmin).Users.Contains<User>(user, new UserComparer());

            if (!isSysAdmin)
            {
                if (action == ActionCommandEnum.Read)
                {
                    bool isSysReader = systemRoleRepo.FindByName(SystemRolesEnum.SysReader).Users.Contains<User>(user, new UserComparer());
                }
            }

            return context;
        }

        /// <summary>
        /// Validate whether the user permission.
        /// </summary>
        /// <param name="user">User requiring the command execution.</param>
        /// <param name="commandPermissions">Permissions requiring to execute the command.</param>
        /// <param name="userPermissions">Permissions of the user.</param>
        /// <returns>A value indicating whether the user has the required permissions.</returns>
        public abstract bool ValidatePermission(User user, IEnumerable<TPermission> commandPermissions, IEnumerable<TPermission> userPermissions);

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
        /// <returns>List of permissions required to execute the actual command.</returns>
        private List<PermissionAssigned> GetPermissionsOfTheCommand(User user, IKernel kernel, Language.SystemCommand command)
        {
            List<PermissionAssigned> requestedPermissions = new List<PermissionAssigned>();
            foreach (Tuple<SystemObjectEnum, string> @object in command.GetUsedSpaceObjects())
            {
                if (string.IsNullOrWhiteSpace(@object.Item2))
                {
                    requestedPermissions.Add(new PermissionOverObjectType(user, @object.Item1, (int)command.PermissionValue, 0));
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