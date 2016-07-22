//-----------------------------------------------------------------------
// <copyright file="UserCacheRepository.cs" company="Integra.Space">
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
    internal class UserCacheRepository : SystemRepositoryBase<User>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserCacheRepository"/> class.
        /// </summary>
        /// <param name="context">Cache context.</param>
        public UserCacheRepository(SystemContext context) : base(context)
        {
        }

        /// <inheritdoc />
        public override IEnumerable<User> List
        {
            get
            {
                return this.Context.Users;
            }
        }

        /// <inheritdoc />
        public override void Add(User entity)
        {
            lock (this.Sync)
            {
                if (this.Context.Users.Exists(x => x.Name == entity.Name))
                {
                    throw new Exception(string.Format("The user '{0}' already exists.", entity.Name));
                }
                else if (this.Context.Users.Exists(x => x.Guid == entity.Guid))
                {
                    throw new Exception(string.Format("The unique identifier '{0}' already exists.", entity.Guid));
                }
                else
                {
                    this.Context.Users.Add(entity);
                }
            }
        }

        /// <inheritdoc />
        public override void Delete(User entity)
        {
            lock (this.Sync)
            {
                if (this.Context.Users.Exists(x => x.Guid == entity.Guid))
                {
                    this.Context.Users.Remove(entity);
                }
                else
                {
                    throw new Exception(string.Format("The user '{0}' don't exists.", entity.Name));
                }
            }
        }

        /// <inheritdoc />
        public override User FindById(Guid id)
        {
            lock (this.Sync)
            {
                return this.Context.Users.Find(x => x.Guid == id);
            }
        }

        /// <inheritdoc />
        public override User FindByName(string name)
        {
            lock (this.Sync)
            {
                return this.Context.Users.Find(x => x.Name == name);
            }
        }
    }
}
