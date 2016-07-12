﻿//-----------------------------------------------------------------------
// <copyright file="StreamCacheRepository.cs" company="Integra.Space">
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
    internal class StreamCacheRepository : CacheRepositoryBase<Stream>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StreamCacheRepository"/> class.
        /// </summary>
        /// <param name="context">Cache context.</param>
        public StreamCacheRepository(CacheContext context) : base(context)
        {
        }

        /// <inheritdoc />
        public override IEnumerable<Stream> List
        {
            get
            {
                return this.Context.Streams;
            }
        }

        /// <inheritdoc />
        public override void Add(Stream entity)
        {
            lock (this.Sync)
            {
                if (this.Context.Streams.Exists(x => x.Identifier == entity.Identifier))
                {
                    throw new Exception(string.Format("The stream '{0}' already exists.", entity.Identifier));
                }
                else if (this.Context.Streams.Exists(x => x.Guid == entity.Guid))
                {
                    throw new Exception(string.Format("The unique identifier '{0}' already exists.", entity.Guid));
                }
                else
                {
                    this.Context.Streams.Add(entity);
                }
            }
        }

        /// <inheritdoc />
        public override void Delete(Stream entity)
        {
            lock (this.Sync)
            {
                if (this.Context.Streams.Exists(x => x.Guid == entity.Guid))
                {
                    this.Context.Streams.Remove(entity);
                }
                else
                {
                    throw new Exception(string.Format("The stream '{0}' don't exists.", entity.Identifier));
                }
            }
        }

        /// <inheritdoc />
        public override Stream FindById(Guid id)
        {
            lock (this.Sync)
            {
                return this.Context.Streams.Find(x => x.Guid == id);
            }
        }

        /// <inheritdoc />
        public override Stream FindByName(string name)
        {
            lock (this.Sync)
            {
                return this.Context.Streams.Find(x => x.Identifier == name);
            }
        }
    }
}
