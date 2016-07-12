//-----------------------------------------------------------------------
// <copyright file="CacheRepositoryBase.cs" company="Integra.Space.Language">
//     Copyright (c) Integra.Space.Language. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Repos
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using Cache;
    using Ninject;

    /// <summary>
    /// Cache repository base class.
    /// </summary>
    /// <typeparam name="TEntity">Entity type.</typeparam>
    internal abstract class CacheRepositoryBase<TEntity> : IRepository<TEntity> where TEntity : class
    {
        /// <summary>
        /// Cache context.
        /// </summary>
        private CacheContext context;

        /// <summary>
        /// Object used to sync up the access to the context objects.
        /// </summary>
        private object sync;

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheRepositoryBase{TEntity}"/> class.
        /// </summary>
        /// <param name="context">Cache context.</param>
        public CacheRepositoryBase(CacheContext context)
        {
            Contract.Assert(context != null);

            this.context = context;
            this.sync = new object();
        }

        /// <inheritdoc />
        public virtual IEnumerable<TEntity> List
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets the cache context.
        /// </summary>
        protected virtual CacheContext Context
        {
            get
            {
                return this.context;
            }
        }

        /// <inheritdoc />
        protected object Sync
        {
            get
            {
                return this.sync;
            }
        }

        /// <inheritdoc />
        public virtual void Add(TEntity entity)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public virtual void Delete(TEntity entity)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public virtual TEntity FindById(Guid id)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public virtual TEntity FindByName(string name)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Update(TEntity entity)
        {
        }
    }
}
