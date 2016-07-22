//-----------------------------------------------------------------------
// <copyright file="Schema.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Models
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Schema class.
    /// </summary>
    internal sealed class Schema : SystemObject
    {
        /// <summary>
        /// Users of this schema.
        /// </summary>
        private ICollection<User> owners;

        /// <summary>
        /// Secure objects of this schema.
        /// </summary>
        private ICollection<SecureObject> secureObjects;

        /// <summary>
        /// Initializes a new instance of the <see cref="Schema"/> class. 
        /// </summary>
        /// <param name="guid">Unique identifier of the schema.</param>
        /// <param name="name">Name of the schema.</param>
        public Schema(Guid guid, string name) : base(guid, name)
        {
            this.owners = new List<User>();
            this.secureObjects = new List<SecureObject>();
        }

        /// <summary>
        /// Gets the users of this schema.
        /// </summary>
        public ICollection<User> Owners
        {
            get
            {
                return this.owners;
            }
        }

        /// <summary>
        /// Gets the secure objects of this schema.
        /// </summary>
        public ICollection<SecureObject> SecureObjects
        {
            get
            {
                return this.secureObjects;
            }
        }
    }
}
