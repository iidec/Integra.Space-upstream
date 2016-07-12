//-----------------------------------------------------------------------
// <copyright file="RoleCacheRepository.cs" company="Integra.Space">
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
    internal class RoleCacheRepository : CacheRepositoryBase<Role>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RoleCacheRepository"/> class.
        /// </summary>
        /// <param name="context">Cache context.</param>
        public RoleCacheRepository(CacheContext context) : base(context)
        {
        }

        /// <inheritdoc />
        public override IEnumerable<Role> List
        {
            get
            {
                return this.Context.Roles;
            }
        }

        /// <inheritdoc />
        public override void Add(Role entity)
        {
            lock (this.Sync)
            {
                if (this.Context.Roles.Exists(x => x.Identifier == entity.Identifier))
                {
                    throw new Exception(string.Format("The source '{0}' already exists.", entity.Identifier));
                }
                else if (this.Context.Roles.Exists(x => x.Guid == entity.Guid))
                {
                    throw new Exception(string.Format("The unique identifier '{0}' already exists.", entity.Guid));
                }
                else
                {
                    this.Context.Roles.Add(entity);
                }
            }
        }

        /// <inheritdoc />
        public override void Delete(Role entity)
        {
            lock (this.Sync)
            {
                if (this.Context.Roles.Exists(x => x.Guid == entity.Guid))
                {
                    this.Context.Roles.Remove(entity);
                }
                else
                {
                    throw new Exception(string.Format("The source '{0}' don't exists.", entity.Identifier));
                }
            }
        }

        /// <inheritdoc />
        public override Role FindById(Guid id)
        {
            lock (this.Sync)
            {
                return this.Context.Roles.Find(x => x.Guid == id);
            }
        }

        /// <inheritdoc />
        public override Role FindByName(string name)
        {
            lock (this.Sync)
            {
                return this.Context.Roles.Find(x => x.Identifier == name);
            }
        }
    }
}
