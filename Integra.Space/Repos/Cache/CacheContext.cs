//-----------------------------------------------------------------------
// <copyright file="CacheContext.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Cache
{
    using System.Collections.Generic;
    using Models;
    
    /// <summary>
    /// Cache context class.
    /// </summary>
    internal class CacheContext
    {
        /// <summary>
        /// Stream repository.
        /// </summary>
        private List<Stream> streams = new List<Stream>();

        /// <summary>
        /// Source repository.
        /// </summary>
        private List<Source> sources = new List<Source>();

        /// <summary>
        /// Role repository.
        /// </summary>
        private List<Role> roles = new List<Role>();

        /// <summary>
        /// User repository.
        /// </summary>
        private List<User> users = new List<User>();

        /// <summary>
        /// Permission repository.
        /// </summary>
        private List<Permission> permissions = new List<Permission>();
        
        /// <summary>
        /// Initializes a new instance of the <see cref="CacheContext"/> class.
        /// </summary>
        public CacheContext()
        {
            this.permissions = new List<Permission>();
            this.users = new List<User>();
            this.roles = new List<Role>();
            this.sources = new List<Source>();
            this.streams = new List<Stream>();
        }

        /// <summary>
        /// Gets the stream repository.
        /// </summary>
        public List<Stream> Streams
        {
            get
            {
                return this.streams;
            }
        }

        /// <summary>
        /// Gets the source repository.
        /// </summary>
        public List<Source> Sources
        {
            get
            {
                return this.sources;
            }
        }

        /// <summary>
        /// Gets the role repository.
        /// </summary>
        public List<Role> Roles
        {
            get
            {
                return this.roles;
            }
        }

        /// <summary>
        /// Gets the user repository.
        /// </summary>
        public List<User> Users
        {
            get
            {
                return this.users;
            }
        }

        /// <summary>
        /// Gets the permission repository.
        /// </summary>
        public List<Permission> Permissions
        {
            get
            {
                return this.permissions;
            }
        }
    }
}
