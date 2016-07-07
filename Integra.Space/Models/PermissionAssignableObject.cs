//-----------------------------------------------------------------------
// <copyright file="PermissionAssignableObject.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Models
{
    /// <summary>
    /// Space object class.
    /// </summary>
    internal class PermissionAssignableObject : SpaceObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PermissionAssignableObject"/> class.
        /// </summary>
        /// <param name="guid">Space object unique identifier.</param>
        /// <param name="identifier">Space object name.</param>
        public PermissionAssignableObject(System.Guid guid, string identifier) : base(guid, identifier)
        {
        }
    }
}
