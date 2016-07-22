//-----------------------------------------------------------------------
// <copyright file="UserXRole.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Models
{
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Permission class.
    /// </summary>
    internal class UserXRole
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserXRole"/> class.
        /// </summary>
        /// <param name="role">Role for the secure object.</param>
        /// <param name="user">Space user.</param>
        public UserXRole(Role role, User user)
        {
            Contract.Assert(role != null);
            Contract.Assert(user != null);

            this.Role = role;
            this.Principal = user;
        }

        /// <summary>
        /// Gets the role for the secure object.
        /// </summary>
        public Role Role { get; private set; }

        /// <summary>
        /// Gets the permission assignable object type.
        /// </summary>
        public Principal Principal { get; private set; }
    }
}
