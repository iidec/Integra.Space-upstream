//-----------------------------------------------------------------------
// <copyright file="UserXRoleCacheRepository.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Repos
{
    using System;
    using System.Collections.Generic;
    using Cache;
    using Models;

    /// <summary>
    /// Space object repository class.
    /// </summary>
    internal class UserXRoleCacheRepository : CacheRepositoryBase<UserXRole>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserXRoleCacheRepository"/> class.
        /// </summary>
        /// <param name="context">Cache context.</param>
        public UserXRoleCacheRepository(CacheContext context) : base(context)
        {
        }

        /// <inheritdoc />
        public override IEnumerable<UserXRole> List
        {
            get
            {
                return this.Context.UsersXRoles;
            }
        }

        /// <inheritdoc />
        public override void Add(UserXRole entity)
        {
            lock (this.Sync)
            {
                if (!this.Context.UsersXRoles.Exists(x => x.Role.Guid == entity.Role.Guid && x.PermissionAssignableObject.Guid == entity.PermissionAssignableObject.Guid))
                {
                    this.Context.UsersXRoles.Add(entity);
                }
            }
        }

        /// <inheritdoc />
        public override void Delete(UserXRole entity)
        {
            lock (this.Sync)
            {
                if (this.Context.UsersXRoles.Exists(x => x.Role.Guid == entity.Role.Guid && x.PermissionAssignableObject.Guid == entity.PermissionAssignableObject.Guid))
                {
                    this.Context.UsersXRoles.Remove(entity);
                }
                else
                {
                    throw new Exception(string.Format("The secure object {0} is not assigned to the role '{1}'.", entity.PermissionAssignableObject.Identifier, entity.Role.Identifier));
                }
            }
        }

        /// <inheritdoc />
        public bool Exists(UserXRole entity)
        {
            lock (this.Sync)
            {
                return this.Context.UsersXRoles.Exists(x => x.PermissionAssignableObject.Guid == entity.PermissionAssignableObject.Guid && x.Role.Guid == entity.Role.Guid);
            }
        }
    }
}
