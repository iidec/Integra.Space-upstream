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
        /// <param name="schema">Schema of the permission for the object type.</param>
        public PermissionOverObjectType(Principal principal, SystemObjectEnum spaceObjectType, int grantValue, int denyValue, Schema schema) : base(principal, grantValue, denyValue)
        {
            Contract.Assert(schema != null);

            this.SpaceObjectType = spaceObjectType;
            this.Schema = schema;
        }

        /// <summary>
        /// Gets schema for the permission over the object type.
        /// </summary>
        public Schema Schema { get; private set; }

        /// <summary>
        /// Gets the space object type.
        /// </summary>
        public SystemObjectEnum SpaceObjectType { get; private set; }
    }
}
