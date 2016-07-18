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
        /// <param name="permissionAssignableObject">Permission assignable space object.</param>
        public UserXRole(Role role, PermissionAssignableObject permissionAssignableObject)
        {
            Contract.Assert(role != null);
            Contract.Assert(permissionAssignableObject != null);

            this.Role = role;
            this.PermissionAssignableObject = permissionAssignableObject;
        }

        /// <summary>
        /// Gets the role for the secure object.
        /// </summary>
        public Role Role { get; private set; }

        /// <summary>
        /// Gets the permission assignable object type.
        /// </summary>
        public PermissionAssignableObject PermissionAssignableObject { get; private set; }
    }
}
