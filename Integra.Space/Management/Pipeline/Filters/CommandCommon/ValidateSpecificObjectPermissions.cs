//-----------------------------------------------------------------------
// <copyright file="ValidateSpecificObjectPermissions.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System.Collections.Generic;
    using System.Linq;
    using Models;
    using Ninject;
    using Repos;

    /// <summary>
    /// Filter lock class.
    /// </summary>
    internal class ValidateSpecificObjectPermissions : ValidatePermissions<PermissionOverSpecificObject>
    {
        /// <inheritdoc />
        public override IEnumerable<PermissionOverSpecificObject> GetUserPermissions(User user, IKernel kernel)
        {
            PermissionCacheRepository<PermissionOverSpecificObject> permissionRepo = kernel.Get<PermissionCacheRepository<PermissionOverSpecificObject>>();

            // obtengo los permisos del usuario.
            IEnumerable<PermissionOverSpecificObject> userPermissions = permissionRepo.List.Where(x => x.Principal.Name == user.Name);

            // obtengo los roles a los que pertenece el usuario.
            UserXRoleCacheRepository rolesRepo = (UserXRoleCacheRepository)kernel.Get<SystemRepositoryBase<UserXRole>>();
            IEnumerable<Role> userRoles = rolesRepo.GetRolesOfTheUser(user);

            IEnumerable<PermissionOverSpecificObject> allRolePermissions = new List<PermissionOverSpecificObject>();

            // obtengo los permisos de cada rol y los agrego a la lista de permisos del usuario
            foreach (Role role in userRoles)
            {
                IEnumerable<PermissionOverSpecificObject> rolePermissions = permissionRepo.List.Where(x => x.Principal.Name == role.Name).Select(x => new PermissionOverSpecificObject(user, x.SpaceObject, x.GrantValue, x.DenyValue));
                userPermissions.Concat(rolePermissions);
            }

            // agrupo los permisos del usuario por objeto para consolidarlos mas adelante
            IEnumerable<IGrouping<SystemObject, PermissionOverSpecificObject>> groupedTotalPermissions = userPermissions.GroupBy(x => x.SpaceObject, new SystemObjectComparer());
            
            List<PermissionOverSpecificObject> result = new List<PermissionOverSpecificObject>();
            foreach (IGrouping<SystemObject, PermissionOverSpecificObject> group in groupedTotalPermissions)
            {
                int grantValue = 0;
                group.Select(x => x.GrantValue).ToList().ForEach(x => grantValue = grantValue | x);

                int denyValue = 0;
                group.Select(x => x.DenyValue).ToList().ForEach(x => denyValue = denyValue | x);

                result.Add(new PermissionOverSpecificObject(user, group.Key, grantValue, denyValue));
            }

            return result;
        }

        /// <inheritdoc />
        public override bool ValidatePermission(IEnumerable<PermissionOverSpecificObject> commandPermissions, IEnumerable<PermissionOverSpecificObject> userPermissions)
        {
            bool isAllowed = false;

            foreach (PermissionOverSpecificObject commandPermission in commandPermissions)
            {
                isAllowed = userPermissions.Contains(commandPermission, new PermissionOverSpecificObjectComparer());
                if (!isAllowed)
                {
                    break;
                }
            }

            return isAllowed;
        }
    }
}