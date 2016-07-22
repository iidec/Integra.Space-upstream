//-----------------------------------------------------------------------
// <copyright file="PermissionAssigned.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Models
{
    /// <summary>
    /// Permission class.
    /// </summary>
    internal abstract class PermissionAssigned
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PermissionAssigned"/> class.
        /// </summary>
        /// <param name="principal">Permission assignable space object.</param>
        /// <param name="grantValue">Grant permission value.</param>
        /// <param name="denyValue">Deny permission value.</param>
        public PermissionAssigned(Principal principal, int grantValue, int denyValue)
        {
            this.Principal = principal;
            this.GrantValue = grantValue;
            this.DenyValue = denyValue;
        }

        /// <summary>
        /// Gets or sets the grant permission values.
        /// </summary>
        public int GrantValue { get; set; }

        /// <summary>
        /// Gets or sets the deny permission values.
        /// </summary>
        public int DenyValue { get; set; }

        /// <summary>
        /// Gets the permission assignable object type.
        /// </summary>
        public Principal Principal { get; private set; }
    }
}
