//-----------------------------------------------------------------------
// <copyright file="CacheContext.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Cache
{
    using System;
    using System.Collections.Generic;
    using Models;

    /// <summary>
    /// Cache context class.
    /// </summary>
    internal static class CacheContext
    {
        /// <summary>
        /// Stream list.
        /// </summary>
        private static List<Stream> streams;

        /// <summary>
        /// Source list.
        /// </summary>
        private static List<Source> sources;

        /// <summary>
        /// Role list.
        /// </summary>
        private static List<Role> roles;

        /// <summary>
        /// User list.
        /// </summary>
        private static List<User> users;

        /// <summary>
        /// Permission list.
        /// </summary>
        private static List<Permission> permissions;

        /// <summary>
        /// Initializes static members of the <see cref="CacheContext"/> class.
        /// </summary>
        static CacheContext()
        {
            streams = new List<Stream>();
            sources = new List<Source>();
            users = new List<User>();
            roles = new List<Role>();
            permissions = new List<Permission>();
        }

        /// <summary>
        /// Gets the stream list.
        /// </summary>
        public static List<Stream> Streams
        {
            get
            {
                return streams;
            }
        }

        /// <summary>
        /// Gets the source list.
        /// </summary>
        public static List<Source> Sources
        {
            get
            {
                return sources;
            }
        }

        /// <summary>
        /// Gets the role list.
        /// </summary>
        public static List<Role> Roles
        {
            get
            {
                return roles;
            }
        }

        /// <summary>
        /// Gets the user list.
        /// </summary>
        public static List<User> Users
        {
            get
            {
                return users;
            }
        }

        /// <summary>
        /// Gets the permission list.
        /// </summary>
        public static List<Permission> Permissions
        {
            get
            {
                return permissions;
            }
        }
    }
}
