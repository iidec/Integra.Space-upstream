//-----------------------------------------------------------------------
// <copyright file="Role.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Models
{
    /// <summary>
    /// Space object class.
    /// </summary>
    internal class Role : PermissionAssignableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Role"/> class.
        /// </summary>
        /// <param name="guid">Space object unique identifier.</param>
        /// <param name="identifier">Space object name.</param>
        public Role(System.Guid guid, string identifier) : base(guid, identifier)
        {
        }
    }
}
