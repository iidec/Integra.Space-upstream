//-----------------------------------------------------------------------
// <copyright file="SourceCacheRepository.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Repos
{
    using System;
    using System.Collections.Generic;
    using Cache;
    using Models;
    using Ninject;

    /// <summary>
    /// Space object repository class.
    /// </summary>
    internal class SourceCacheRepository : CacheRepositoryBase<Source>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SourceCacheRepository"/> class.
        /// </summary>
        /// <param name="context">Cache context.</param>
        public SourceCacheRepository(CacheContext context) : base(context)
        {
        }

        /// <inheritdoc />
        public override IEnumerable<Source> List
        {
            get
            {
                return this.Context.Sources;
            }
        }

        /// <inheritdoc />
        public override void Add(Source entity)
        {
            lock (this.Sync)
            {
                if (this.Context.Sources.Exists(x => x.Identifier == entity.Identifier))
                {
                    throw new Exception(string.Format("The source '{0}' already exists.", entity.Identifier));
                }
                else if (this.Context.Sources.Exists(x => x.Guid == entity.Guid))
                {
                    throw new Exception(string.Format("The unique identifier '{0}' already exists.", entity.Guid));
                }
                else
                {
                    this.Context.Sources.Add(entity);
                }
            }
        }

        /// <inheritdoc />
        public override void Delete(Source entity)
        {
            lock (this.Sync)
            {
                if (this.Context.Sources.Exists(x => x.Guid == entity.Guid))
                {
                    this.Context.Sources.Remove(entity);
                }
                else
                {
                    throw new Exception(string.Format("The source '{0}' don't exists.", entity.Identifier));
                }
            }
        }

        /// <inheritdoc />
        public override Source FindById(Guid id)
        {
            lock (this.Sync)
            {
                return this.Context.Sources.Find(x => x.Guid == id);
            }
        }

        /// <inheritdoc />
        public override Source FindByName(string name)
        {
            lock (this.Sync)
            {
                return this.Context.Sources.Find(x => x.Identifier == name);
            }
        }
    }
}
