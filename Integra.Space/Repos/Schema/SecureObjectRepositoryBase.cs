//-----------------------------------------------------------------------
// <copyright file="SecureObjectRepositoryBase.cs" company="Integra.Space.Language">
//     Copyright (c) Integra.Space.Language. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Repos
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using Cache;
    using Models;
    using Ninject;

    /// <summary>
    /// Cache repository base class.
    /// </summary>
    /// <typeparam name="TEntity">Entity type.</typeparam>
    internal abstract class SecureObjectRepositoryBase<TEntity> : IRepository<TEntity> where TEntity : SecureObject
    {
        /// <summary>
        /// Cache context.
        /// </summary>
        private SystemContext context;

        /// <summary>
        /// Object used to sync up the access to the context objects.
        /// </summary>
        private object sync;

        /// <summary>
        /// Initializes a new instance of the <see cref="SecureObjectRepositoryBase{TEntity}"/> class.
        /// </summary>
        /// <param name="context">Cache context.</param>
        public SecureObjectRepositoryBase(SystemContext context)
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
        protected virtual SystemContext Context
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
