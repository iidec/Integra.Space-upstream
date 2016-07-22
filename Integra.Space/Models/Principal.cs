//-----------------------------------------------------------------------
// <copyright file="Principal.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Models
{
    using System.Collections.Generic;

    /// <summary>
    /// Space object class.
    /// </summary>
    internal class Principal : SystemObject
    {
        /// <summary>
        /// Permissions assigned to the principal.
        /// </summary>
        private ICollection<PermissionAssigned> permissions;

        /// <summary>
        /// Initializes a new instance of the <see cref="Principal"/> class.
        /// </summary>
        /// <param name="guid">Space object unique identifier.</param>
        /// <param name="identifier">Space object name.</param>
        public Principal(System.Guid guid, string identifier) : base(guid, identifier)
        {
            this.permissions = new List<PermissionAssigned>();
        }

        /// <summary>
        /// Gets the permissions assigned to the principal.
        /// </summary>
        public ICollection<PermissionAssigned> Permissions
        {
            get
            {
                return this.permissions;
            }
        }
    }
}
