//-----------------------------------------------------------------------
// <copyright file="PermissionOverObjectType.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Models
{
    using System.Diagnostics.Contracts;
    using Common;

    /// <summary>
    /// Permission class.
    /// </summary>
    internal class PermissionOverObjectType : PermissionAssigned
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PermissionOverObjectType"/> class.
        /// </summary>
        /// <param name="principal">Permission assignable space object.</param>
        /// <param name="spaceObjectType">Space object type.</param>
        /// <param name="grantValue">Grant permission value.</param>
        /// <param name="denyValue">Deny permission value.</param>
        public PermissionOverObjectType(Principal principal, SystemObjectEnum spaceObjectType, int grantValue, int denyValue) : base(principal, grantValue, denyValue)
        {
            this.SpaceObjectType = spaceObjectType;
        }
        
        /// <summary>
        /// Gets the space object type.
        /// </summary>
        public SystemObjectEnum SpaceObjectType { get; private set; }
    }
}
