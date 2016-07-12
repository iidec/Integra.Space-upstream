//-----------------------------------------------------------------------
// <copyright file="User.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Models
{
    using System;

    /// <summary>
    /// Space object class.
    /// </summary>
    internal class User : PermissionAssignableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="User"/> class.
        /// </summary>
        /// <param name="guid">Space object unique identifier.</param>
        /// <param name="identifier">Space object name.</param>
        /// <param name="password">Password of the user.</param>
        /// <param name="enable">Flag that indicates whether the user is enable.</param>
        public User(System.Guid guid, string identifier, string password, bool enable) : base(guid, identifier)
        {
            this.Password = password;
            this.Enable = enable;
        }

        /// <summary>
        /// Gets or sets the password of the user.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user is enable.
        /// </summary>
        public bool Enable { get; set; }
    }
}
