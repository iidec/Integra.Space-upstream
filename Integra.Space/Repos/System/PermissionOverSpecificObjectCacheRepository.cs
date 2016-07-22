//-----------------------------------------------------------------------
// <copyright file="PermissionOverSpecificObjectCacheRepository.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Repos
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Cache;
    using Common;
    using Models;

    /// <summary>
    /// Space object repository class.
    /// </summary>
    internal class PermissionOverSpecificObjectCacheRepository : PermissionCacheRepository<PermissionOverSpecificObject>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PermissionOverSpecificObjectCacheRepository"/> class.
        /// </summary>
        /// <param name="context">Cache context.</param>
        public PermissionOverSpecificObjectCacheRepository(SystemContext context) : base(context)
        {
        }
        
        /// <inheritdoc />
        public override IEnumerable<PermissionOverSpecificObject> List
        {
            get
            {
                return this.Context.PermissionsOverSpecificObject;
            }
        }

        /// <summary>
        /// Grant a permission.
        /// </summary>
        /// <param name="entity">Permission to grant.</param>
        public override void Grant(PermissionOverSpecificObject entity)
        {
            lock (this.Sync)
            {
                PermissionOverSpecificObject permission = this.GetPermission(entity);

                if (permission == null)
                {
                    this.Context.PermissionsOverSpecificObject.Add(entity);
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
        public override void Deny(PermissionOverSpecificObject entity)
        {
            lock (this.Sync)
            {
                PermissionOverSpecificObject permission = this.GetPermission(entity);

                if (permission == null)
                {
                    this.Context.PermissionsOverSpecificObject.Add(entity);
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
        public override PermissionOverSpecificObject GetPermission(PermissionOverSpecificObject permission)
        {
            SystemObjectEnum permissionObjectType;
            Enum.TryParse<SystemObjectEnum>(permission.SpaceObject.Name, true, out permissionObjectType);

            return this.GetPermissionsForPrincipal(permission.Principal)
                .Where(x => x is PermissionOverSpecificObject)
                .Cast<PermissionOverSpecificObject>()
                .SingleOrDefault(x =>
                {
                    SystemObjectEnum actualObjectType;
                    Enum.TryParse<SystemObjectEnum>(x.SpaceObject.Name, true, out actualObjectType);
                    if (actualObjectType == permissionObjectType && x.SpaceObject.Name == permission.SpaceObject.Name)
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
        protected IEnumerable<PermissionAssigned> GetPermissionsForPrincipal(Principal principal)
        {
            return this.Context.PermissionsOverSpecificObject
                .Where(x =>
                {
                    if (x.Principal is User)
                    {
                        User user = (User)x.Principal;
                        if (this.Context.PermissionsOverSpecificObject.Exists(u => u.Principal.Name == user.Name))
                        {
                            return true;
                        }
                    }
                    else if (x.Principal is Role)
                    {
                        Role role = (Role)x.Principal;
                        if (this.Context.PermissionsOverSpecificObject.Exists(r => r.Principal.Name == role.Name))
                        {
                            return true;
                        }
                    }

                    return false;
                });
        }
    }
}
