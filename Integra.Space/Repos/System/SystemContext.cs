//-----------------------------------------------------------------------
// <copyright file="SystemContext.cs" company="Integra.Space">
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
    internal class SystemContext
    {
        /// <summary>
        /// Source repository.
        /// </summary>
        private List<Schema> schemas = new List<Schema>();
        
        /// <summary>
        /// Role repository.
        /// </summary>
        private List<Role> roles = new List<Role>();

        /// <summary>
        /// Role repository.
        /// </summary>
        private List<SystemRole> systemRoles = new List<SystemRole>();

        /// <summary>
        /// User repository.
        /// </summary>
        private List<User> users = new List<User>();

        /// <summary>
        /// Secure objects by role repository.
        /// </summary>
        private List<UserXRole> usersXRoles = new List<UserXRole>();

        /// <summary>
        /// Stream repository.
        /// </summary>
        private List<Stream> streams = new List<Stream>();

        /// <summary>
        /// Source repository.
        /// </summary>
        private List<Source> sources = new List<Source>();
                
        /// <summary>
        /// Permission repository.
        /// </summary>
        private List<PermissionOverSpecificObject> permissionsOverSpecificObject = new List<PermissionOverSpecificObject>();
        
        /// <summary>
        /// Permission repository.
        /// </summary>
        private List<PermissionOverObjectType> permissionsOverObjectType = new List<PermissionOverObjectType>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemContext"/> class.
        /// </summary>
        public SystemContext()
        {
            this.systemRoles = new List<SystemRole>();
            this.sources = new List<Source>();
            this.streams = new List<Stream>();
            this.users = new List<User>();
            this.roles = new List<Role>();
            this.usersXRoles = new List<UserXRole>();
            this.permissionsOverObjectType = new List<PermissionOverObjectType>();
            this.permissionsOverSpecificObject = new List<PermissionOverSpecificObject>();
        }

        /// <summary>
        /// Gets the source repository.
        /// </summary>
        public List<Schema> Schemas
        {
            get
            {
                return this.schemas;
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
        /// Gets the system role repository.
        /// </summary>
        public List<SystemRole> SystemRoles
        {
            get
            {
                return this.systemRoles;
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
        /// Gets the secure objects by roles repository.
        /// </summary>
        public List<UserXRole> UsersXRoles
        {
            get
            {
                return this.usersXRoles;
            }
        }

        /// <summary>
        /// Gets the permission repository.
        /// </summary>
        public List<PermissionOverSpecificObject> PermissionsOverSpecificObject
        {
            get
            {
                return this.permissionsOverSpecificObject;
            }
        }

        /// <summary>
        /// Gets the permission repository.
        /// </summary>
        public List<PermissionOverObjectType> PermissionsOverObjectType
        {
            get
            {
                return this.permissionsOverObjectType;
            }
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
    }
}
