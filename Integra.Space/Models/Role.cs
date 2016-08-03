//-----------------------------------------------------------------------
// <copyright file="Role.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Models
{
    using System.Collections.Generic;
    using Integra.Space.Common;

    /// <summary>
    /// Space object class.
    /// </summary>
    internal class Role : Principal
    {
        /// <summary>
        /// Users of the role.
        /// </summary>
        private ICollection<User> users;

        /// <summary>
        /// Initializes a new instance of the <see cref="Role"/> class.
        /// </summary>
        /// <param name="guid">Space object unique identifier.</param>
        /// <param name="identifier">Space object name.</param>
        public Role(System.Guid guid, string identifier) : base(guid, identifier)
        {
            this.users = new List<User>();
        }

        /// <summary>
        /// Gets the users of the role.
        /// </summary>
        public ICollection<User> Users
        {
            get
            {
                return this.users;
            }
        }
    }
}
