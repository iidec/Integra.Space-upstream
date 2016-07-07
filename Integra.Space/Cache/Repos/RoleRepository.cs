//-----------------------------------------------------------------------
// <copyright file="RoleRepository.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Cache
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Models;

    /// <summary>
    /// Space object repository class.
    /// </summary>
    internal class RoleRepository : ICacheRepository<Role>
    {
        /// <summary>
        /// Specify context.
        /// </summary>
        private List<Role> listOfObjects = null;

        /// <summary>
        /// Object used to sync up the access to the context objects.
        /// </summary>
        private object sync;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoleRepository"/> class.
        /// </summary>
        public RoleRepository()
        {
            try
            {
                this.listOfObjects = (List<Role>)typeof(CacheContext).GetProperties().First(x => x.PropertyType == typeof(Role)).GetGetMethod().Invoke(null, new object[] { });
            }
            catch (Exception e)
            {
                throw new Exception("Space Dataset not defined in the context.", e);
            }

            this.sync = new object();
        }

        /// <inheritdoc />
        public IEnumerable<Role> List
        {
            get
            {
                return this.listOfObjects;
            }
        }

        /// <inheritdoc />
        public void Add(Role entity)
        {
            lock (this.sync)
            {
                this.listOfObjects.Add(entity);
            }
        }

        /// <inheritdoc />
        public void Delete(Role entity)
        {
            lock (this.sync)
            {
                this.listOfObjects.Remove(entity);
            }
        }

        /// <inheritdoc />
        public Role FindById(Guid id)
        {
            lock (this.sync)
            {
                return this.listOfObjects.Find(x => x.Guid == id);
            }
        }

        /// <inheritdoc />
        public Role FindByName(string name)
        {
            lock (this.sync)
            {
                return this.listOfObjects.Find(x => x.Identifier == name);
            }
        }
    }
}
