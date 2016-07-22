//-----------------------------------------------------------------------
// <copyright file="SystemRole.cs" company="Integra.Space">
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
    internal class SystemRole
    {
        /// <summary>
        /// Role type enumerator.
        /// </summary>
        private SystemRolesEnum roleType;

        /// <summary>
        /// User assigned to the system role.
        /// </summary>
        private ICollection<User> users;

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemRole"/> class.
        /// </summary>
        /// <param name="roleType">Role type.</param>
        public SystemRole(SystemRolesEnum roleType)
        {
            this.roleType = roleType;
            this.users = new List<User>();
        }

        /// <summary>
        /// Gets the role type of the role.
        /// </summary>
        public SystemRolesEnum RoleType
        {
            get
            {
                return this.roleType;
            }
        }

        /// <summary>
        /// Gets the users assigned to the system role.
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
