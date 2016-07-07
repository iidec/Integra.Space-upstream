//-----------------------------------------------------------------------
// <copyright file="SourceCacheRepository.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Repos
{
    using System;
    using System.Collections.Generic;
    using Models;

    /// <summary>
    /// Space object repository class.
    /// </summary>
    internal class SourceCacheRepository : CacheRepositoryBase<Source>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SourceCacheRepository"/> class.
        /// </summary>
        /// <param name="sourceList">List of sources.</param>
        public SourceCacheRepository(List<Source> sourceList) : base(sourceList)
        {
        }

        /// <inheritdoc />
        public override void Add(Source entity)
        {
            lock (this.Sync)
            {
                if (this.ListOfObjects.Exists(x => x.Guid == entity.Guid))
                {
                    throw new Exception(string.Format("The source {0} already exists.", entity.Identifier));
                }
                else if (this.ListOfObjects.Exists(x => x.Identifier == entity.Identifier))
                {
                    throw new Exception(string.Format("The unique identifier {0} already exists.", entity.Guid));
                }
                else
                {
                    this.ListOfObjects.Add(entity);
                }
            }
        }

        /// <inheritdoc />
        public override void Delete(Source entity)
        {
            lock (this.Sync)
            {
                if (this.ListOfObjects.Exists(x => x.Guid == entity.Guid))
                {
                    throw new Exception(string.Format("The source {0} don't exists.", entity.Identifier));
                }
                else if (this.ListOfObjects.Exists(x => x.Identifier == entity.Identifier))
                {
                    throw new Exception(string.Format("The unique identifier {0} don't exists.", entity.Guid));
                }
                else
                {
                    this.ListOfObjects.Remove(entity);
                }
            }
        }

        /// <inheritdoc />
        public override Source FindById(Guid id)
        {
            lock (this.Sync)
            {
                return this.ListOfObjects.Find(x => x.Guid == id);
            }
        }

        /// <inheritdoc />
        public override Source FindByName(string name)
        {
            lock (this.Sync)
            {
                return this.ListOfObjects.Find(x => x.Identifier == name);
            }
        }
    }
}
