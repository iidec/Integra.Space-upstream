//-----------------------------------------------------------------------
// <copyright file="PermissionOverObjectTypeCacheRepository.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Repos
{
    using System.Collections.Generic;
    using System.Linq;
    using Cache;
    using Models;

    /// <summary>
    /// Space object repository class.
    /// </summary>
    internal class PermissionOverObjectTypeCacheRepository : PermissionCacheRepository<PermissionOverObjectType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PermissionOverObjectTypeCacheRepository"/> class.
        /// </summary>
        /// <param name="context">Cache context.</param>
        public PermissionOverObjectTypeCacheRepository(SystemContext context) : base(context)
        {
        }

        /// <inheritdoc />
        public override IEnumerable<PermissionOverObjectType> List
        {
            get
            {
                return this.Context.PermissionsOverObjectType;
            }
        }

        /// <summary>
        /// Grant a permission.
        /// </summary>
        /// <param name="entity">Permission to grant.</param>
        public override void Grant(PermissionOverObjectType entity)
        {
            lock (this.Sync)
            {
                PermissionOverObjectType permission = this.GetPermission(entity);

                if (permission == null)
                {
                    this.Context.PermissionsOverObjectType.Add(entity);
                    entity.Principal.Permissions.Add(entity);
                }
                else
                {
                    permission.GrantValue = permission.GrantValue | entity.GrantValue;
                }
            }
        }

        /// <summary>
        /// Deny a permission.
        /// </summary>
        /// <param name="entity">Permission to deny.</param>
        public override void Deny(PermissionOverObjectType entity)
        {
            lock (this.Sync)
            {
                PermissionOverObjectType permission = this.GetPermission(entity);

                if (permission == null)
                {
                    this.Context.PermissionsOverObjectType.Add(entity);
                    entity.Principal.Permissions.Add(entity);
                }
                else
                {
                    // permission.GrantValue = permission.GrantValue ^ entity.GrantValue;
                    permission.DenyValue = permission.DenyValue | entity.DenyValue;
                }
            }
        }

        /// <summary>
        /// Get the required permission.
        /// </summary>
        /// <param name="permission">Permission to find.</param>
        /// <returns>The required permission.</returns>
        public override PermissionOverObjectType GetPermission(PermissionOverObjectType permission)
        {
            return this.GetPermissionsForPrincipal(permission.Principal)
                .Where(x => x is PermissionOverObjectType)
                .Cast<PermissionOverObjectType>()
                .SingleOrDefault(x =>
                {
                    if (x.SpaceObjectType == permission.SpaceObjectType)
                    {
                        return true;
                    }

                    return false;
                });
        }
        
        /// <summary>
        /// Gets the permission for the specified principal.
        /// </summary>
        /// <param name="principal">Principal entity.</param>
        /// <returns>The list of permissions.</returns>
        protected IEnumerable<PermissionOverObjectType> GetPermissionsForPrincipal(Principal principal)
        {
            return this.Context.PermissionsOverObjectType
                .Where(x =>
                {
                    if (x.Principal is User)
                    {
                        User user = (User)x.Principal;
                        if (this.Context.PermissionsOverObjectType.Exists(u => u.Principal.Name == user.Name))
                        {
                            return true;
                        }
                    }
                    else if (x.Principal is Role)
                    {
                        Role role = (Role)x.Principal;
                        if (this.Context.PermissionsOverObjectType.Exists(r => r.Principal.Name == role.Name))
                        {
                            return true;
                        }
                    }

                    return false;
                });
        }
    }
}
