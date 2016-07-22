//-----------------------------------------------------------------------
// <copyright file="SchemaCacheRepository.cs" company="Integra.Space">
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
    internal class SchemaCacheRepository : SystemRepositoryBase<Schema>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaCacheRepository"/> class.
        /// </summary>
        /// <param name="context">Cache context.</param>
        public SchemaCacheRepository(SystemContext context) : base(context)
        {
        }

        /// <inheritdoc />
        public override IEnumerable<Schema> List
        {
            get
            {
                return this.Context.Schemas;
            }
        }

        /// <inheritdoc />
        public override void Add(Schema entity)
        {
            lock (this.Sync)
            {
                if (this.Context.Schemas.Exists(x => x.Name == entity.Name))
                {
                    throw new Exception(string.Format("The source '{0}' already exists.", entity.Name));
                }
                else if (this.Context.Schemas.Exists(x => x.Guid == entity.Guid))
                {
                    throw new Exception(string.Format("The unique identifier '{0}' already exists.", entity.Guid));
                }
                else
                {
                    this.Context.Schemas.Add(entity);
                }
            }
        }

        /// <inheritdoc />
        public override void Delete(Schema entity)
        {
            lock (this.Sync)
            {
                if (this.Context.Schemas.Exists(x => x.Guid == entity.Guid))
                {
                    this.Context.Schemas.Remove(entity);
                }
                else
                {
                    throw new Exception(string.Format("The source '{0}' don't exists.", entity.Name));
                }
            }
        }

        /// <inheritdoc />
        public override Schema FindById(Guid id)
        {
            lock (this.Sync)
            {
                return this.Context.Schemas.Find(x => x.Guid == id);
            }
        }

        /// <inheritdoc />
        public override Schema FindByName(string name)
        {
            lock (this.Sync)
            {
                return this.Context.Schemas.Find(x => x.Name == name);
            }
        }
    }
}
