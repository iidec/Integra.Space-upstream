//-----------------------------------------------------------------------
// <copyright file="PermissionCacheRepository.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Repos
{
    using Cache;
    using Models;

    /// <summary>
    /// Space object repository class.
    /// </summary>
    /// <typeparam name="TPermission">Permission type.</typeparam>
    internal abstract class PermissionCacheRepository<TPermission> : SystemRepositoryBase<TPermission> where TPermission : PermissionAssigned
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PermissionCacheRepository{TPermission}"/> class.
        /// </summary>
        /// <param name="context">Cache context.</param>
        public PermissionCacheRepository(SystemContext context) : base(context)
        {
        }

        /// <summary>
        /// Grant a permission.
        /// </summary>
        /// <param name="entity">Permission to grant.</param>
        public abstract void Grant(TPermission entity);

        /// <summary>
        /// Deny a permission.
        /// </summary>
        /// <param name="entity">Permission to deny.</param>
        public abstract void Deny(TPermission entity);

        /// <summary>
        /// Revoke a permission.
        /// </summary>
        /// <param name="entity">Permission to revoke.</param>
        public void Revoke(TPermission entity)
        {
            lock (this.Sync)
            {
                PermissionAssigned permission = this.GetPermission(entity);

                if (permission != null)
                {
                    if (permission.GrantValue < entity.GrantValue)
                    {
                        permission.GrantValue = 0;
                    }
                    else
                    {
                        permission.GrantValue = permission.GrantValue - entity.GrantValue;
                        permission.DenyValue = permission.DenyValue - entity.DenyValue;
                    }
                }
            }
        }
        
        /// <summary>
        /// Reverse the permission to an old value.
        /// </summary>
        /// <param name="entity">Permission with the old value.</param>
        public void ReverseGrant(TPermission entity)
        {
            lock (this.Sync)
            {
                TPermission permission = this.GetPermission(entity);

                if (permission != null)
                {
                    permission.GrantValue = permission.GrantValue & entity.GrantValue;
                }
            }
        }

        /// <summary>
        /// Reverse the permission to an old value.
        /// </summary>
        /// <param name="entity">Permission with the old value.</param>
        public void ReverseDeny(TPermission entity)
        {
            lock (this.Sync)
            {
                TPermission permission = this.GetPermission(entity);

                if (permission != null)
                {
                    // permission.GrantValue = permission.GrantValue ^ entity.GrantValue;
                    permission.DenyValue = permission.DenyValue & entity.DenyValue;
                }
            }
        }

        /// <summary>
        /// Reverse the permission to an old value.
        /// </summary>
        /// <param name="entity">Permission with the old value.</param>
        public void ReverseRevoke(TPermission entity)
        {
            lock (this.Sync)
            {
                TPermission permission = this.GetPermission(entity);

                if (permission != null)
                {
                    permission.GrantValue = permission.GrantValue + entity.GrantValue;
                    permission.DenyValue = permission.DenyValue + entity.DenyValue;
                }
            }
        }

        /// <summary>
        /// Get the required permission.
        /// </summary>
        /// <param name="permission">Permission to find.</param>
        /// <returns>The required permission.</returns>
        public abstract TPermission GetPermission(TPermission permission);
    }
}
