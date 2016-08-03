//-----------------------------------------------------------------------
// <copyright file="ValidateObjectTypePermissions.cs" company="Integra.Space">
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
    internal class ValidateObjectTypePermissions : ValidatePermissions<PermissionOverObjectType>
    {
        /// <inheritdoc />
        public override IEnumerable<PermissionOverObjectType> GetUserPermissions(User user, IKernel kernel)
        {
            PermissionCacheRepository<PermissionOverObjectType> permissionRepo = kernel.Get<PermissionCacheRepository<PermissionOverObjectType>>();

            // obtengo los permisos del usuario.
            IEnumerable<PermissionOverObjectType> userPermissions = permissionRepo.List.Where(x => x.Principal.Name == user.Name);

            // obtengo los roles a los que pertenece el usuario.
            UserXRoleCacheRepository rolesRepo = (UserXRoleCacheRepository)kernel.Get<SystemRepositoryBase<UserXRole>>();
            IEnumerable<Role> userRoles = rolesRepo.GetRolesOfTheUser(user);

            IEnumerable<PermissionOverObjectType> allRolePermissions = new List<PermissionOverObjectType>();

            // obtengo los permisos de cada rol y los agrego a la lista de permisos del usuario
            foreach (Role role in userRoles)
            {
                IEnumerable<PermissionOverObjectType> rolePermissions = permissionRepo.List
                    .Where(x => x.Principal.Name == role.Name)
                    .Select(x => new PermissionOverObjectType(user, x.SpaceObjectType, x.GrantValue, x.DenyValue, x.Schema));
                userPermissions.Concat(rolePermissions);
            }

            // agrupo los permisos del usuario por objeto para consolidarlos mas adelante
            IEnumerable<IGrouping<dynamic, PermissionOverObjectType>> groupedTotalPermissions = userPermissions.GroupBy(x => new { ObjectType = x.SpaceObjectType, SchemaName = x.Schema.Name });

            // consolido 
            List<PermissionOverObjectType> result = new List<PermissionOverObjectType>();
            foreach (IGrouping<dynamic, PermissionOverObjectType> group in groupedTotalPermissions)
            {
                int grantValue = 0;
                group.Select(x => x.GrantValue).ToList().ForEach(x => grantValue = grantValue | x);

                int denyValue = 0;
                group.Select(x => x.DenyValue).ToList().ForEach(x => denyValue = denyValue | x);

                PermissionOverObjectType permissionAux = group.First();
                result.Add(new PermissionOverObjectType(user, (Common.SystemObjectEnum)group.Key.ObjectType, grantValue, denyValue, permissionAux.Schema));
            }

            return result;
        }

        /// <inheritdoc />
        public override bool ValidatePermission(IEnumerable<PermissionOverObjectType> commandPermissions, IEnumerable<PermissionOverObjectType> userPermissions)
        {
            bool isAllowed = false;

            foreach (PermissionOverObjectType commandPermission in commandPermissions)
            {
                isAllowed = userPermissions.Contains(commandPermission, new PermissionOverObjectTypeComparer());
                if (!isAllowed)
                {
                    break;
                }
            }

            return isAllowed;
        }
    }
}