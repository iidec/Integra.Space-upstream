//-----------------------------------------------------------------------
// <copyright file="SourceRepository.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Cache
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using Models;

    /// <summary>
    /// Space object repository class.
    /// </summary>
    internal class SourceRepository : ICacheRepository<Source>
    {
        /// <summary>
        /// Specify context.
        /// </summary>
        private List<Source> listOfObjects;

        /// <summary>
        /// Object used to sync up the access to the context objects.
        /// </summary>
        private object sync;

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceRepository"/> class.
        /// </summary>
        /// <param name="sourceList">List of sources.</param>
        public SourceRepository(List<Source> sourceList)
        {
            Contract.Assert(sourceList != null);

            this.listOfObjects = sourceList;
            this.sync = new object();
        }

        /// <inheritdoc />
        public IEnumerable<Source> List
        {
            get
            {
                return this.listOfObjects;
            }
        }

        /// <inheritdoc />
        public void Add(Source entity)
        {
            lock (this.sync)
            {
                if (this.listOfObjects.Exists(x => x.Guid == entity.Guid))
                {
                    throw new Exception(string.Format("The source {0} already exists.", entity.Identifier));
                }
                else if (this.listOfObjects.Exists(x => x.Identifier == entity.Identifier))
                {
                    throw new Exception(string.Format("The unique identifier {0} already exists.", entity.Guid));
                }
                else
                {
                    this.listOfObjects.Add(entity);
                }
            }
        }

        /// <inheritdoc />
        public void Delete(Source entity)
        {
            lock (this.sync)
            {
                if (this.listOfObjects.Exists(x => x.Guid == entity.Guid))
                {
                    throw new Exception(string.Format("The source {0} don't exists.", entity.Identifier));
                }
                else if (this.listOfObjects.Exists(x => x.Identifier == entity.Identifier))
                {
                    throw new Exception(string.Format("The unique identifier {0} don't exists.", entity.Guid));
                }
                else
                {
                    this.listOfObjects.Remove(entity);
                }
            }
        }

        /// <inheritdoc />
        public Source FindById(Guid id)
        {
            lock (this.sync)
            {
                return this.listOfObjects.Find(x => x.Guid == id);
            }
        }

        /// <inheritdoc />
        public Source FindByName(string name)
        {
            lock (this.sync)
            {
                return this.listOfObjects.Find(x => x.Identifier == name);
            }
        }
    }
}
