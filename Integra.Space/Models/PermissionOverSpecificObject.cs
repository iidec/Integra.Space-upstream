//-----------------------------------------------------------------------
// <copyright file="PermissionOverSpecificObject.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Models
{
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Permission class.
    /// </summary>
    internal class PermissionOverSpecificObject : PermissionAssigned
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PermissionOverSpecificObject"/> class.
        /// </summary>
        /// <param name="principal">Permission assignable space object.</param>
        /// <param name="spaceObject">Space object.</param>
        /// <param name="grantValue">Grant permission value.</param>
        /// <param name="denyValue">Deny permission value.</param>
        public PermissionOverSpecificObject(Principal principal, SystemObject spaceObject, int grantValue, int denyValue) : base(principal, grantValue, denyValue)
        {
            this.SpaceObject = spaceObject;
        }

        /// <summary>
        /// Gets the space object.
        /// </summary>
        public SystemObject SpaceObject { get; private set; }
    }
}
