//-----------------------------------------------------------------------
// <copyright file="PermissionCacheRepository.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Repos
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Cache;
    using Models;

    /// <summary>
    /// Space object repository class.
    /// </summary>
    internal class PermissionCacheRepository : CacheRepositoryBase<Permission>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PermissionCacheRepository"/> class.
        /// </summary>
        /// <param name="context">Cache context.</param>
        public PermissionCacheRepository(CacheContext context) : base(context)
        {
        }

        /// <inheritdoc />
        public override IEnumerable<Permission> List
        {
            get
            {
                return this.Context.Permissions;
            }
        }

        /// <summary>
        /// Grant a permission.
        /// </summary>
        /// <param name="entity">Permission to grant.</param>
        public void Grant(Permission entity)
        {
            lock (this.Sync)
            {
                Permission permission = this.GetPermission(entity);

                if (permission == null)
                {
                    this.Context.Permissions.Add(entity);
                }
                else
                {
                    permission.Value = permission.Value | entity.Value;
                }
            }
        }

        /// <summary>
        /// Revoke a permission.
        /// </summary>
        /// <param name="entity">Permission to revoke.</param>
        public void Revoke(Permission entity)
        {
            lock (this.Sync)
            {
                Permission permission = this.GetPermission(entity);

                if (permission != null)
                {
                    if (permission.Value < entity.Value)
                    {
                        permission.Value = 0;
                    }
                    else
                    {
                        permission.Value = permission.Value - entity.Value;
                    }
                }
            }
        }

        /// <summary>
        /// Deny a permission.
        /// </summary>
        /// <param name="entity">Permission to deny.</param>
        public void Deny(Permission entity)
        {
            lock (this.Sync)
            {
                Permission permission = this.GetPermission(entity);

                if (permission == null)
                {
                    this.Context.Permissions.Add(entity);
                }
                else
                {
                    permission.Value = permission.Value ^ entity.Value;
                }
            }
        }

        /// <summary>
        /// Reverse the permission to an old value.
        /// </summary>
        /// <param name="entity">Permission with the old value.</param>
        public void ReverseGrant(Permission entity)
        {
            lock (this.Sync)
            {
                Permission permission = this.GetPermission(entity);

                if (permission != null)
                {
                    permission.Value = permission.Value & entity.Value;
                }                
            }
        }

        /// <summary>
        /// Reverse the permission to an old value.
        /// </summary>
        /// <param name="entity">Permission with the old value.</param>
        public void ReverseDeny(Permission entity)
        {
            lock (this.Sync)
            {
                Permission permission = this.GetPermission(entity);

                if (permission != null)
                {
                    permission.Value = permission.Value ^ entity.Value;
                }
            }
        }

        /// <summary>
        /// Reverse the permission to an old value.
        /// </summary>
        /// <param name="entity">Permission with the old value.</param>
        public void ReverseRevoke(Permission entity)
        {
            lock (this.Sync)
            {
                Permission permission = this.GetPermission(entity);

                if (permission != null)
                {
                    permission.Value = permission.Value + entity.Value;
                }
            }
        }

        /// <summary>
        /// Get the required permission.
        /// </summary>
        /// <param name="entity">Permission to search.</param>
        /// <returns>The permission required.</returns>
        public Permission GetPermission(Permission entity)
        {
            return this.Context.Permissions.FirstOrDefault(x =>
            {
                if (x.PermissionAssignableObject is User)
                {
                    User user = (User)x.PermissionAssignableObject;
                    if (!this.Context.Permissions.Exists(u => u.PermissionAssignableObject.Identifier == user.Identifier))
                    {
                        return false;
                    }
                }
                else if (x.PermissionAssignableObject is Role)
                {
                    Role role = (Role)x.PermissionAssignableObject;
                    if (!this.Context.Permissions.Exists(r => r.PermissionAssignableObject.Identifier == role.Identifier))
                    {
                        return false;
                    }
                }

                if (x.SpaceObjectType == entity.SpaceObjectType)
                {
                    if (entity.SpaceObject != null && (entity.SpaceObjectType == Common.SpaceObjectEnum.Source || entity.SpaceObjectType == Common.SpaceObjectEnum.Stream))
                    {
                        if (x.SpaceObject.Identifier == entity.SpaceObject.Identifier)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        return true;
                    }
                }

                return false;
            });
        }
    }
}
