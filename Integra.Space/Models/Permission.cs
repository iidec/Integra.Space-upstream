//-----------------------------------------------------------------------
// <copyright file="Permission.cs" company="Integra.Space">
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
    internal class Permission
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Permission"/> class.
        /// </summary>
        /// <param name="permissionAssignableObject">Permission assignable space object.</param>
        /// <param name="spaceObjectType">Space object type.</param>
        /// <param name="value">Permission value.</param>
        public Permission(PermissionAssignableObject permissionAssignableObject, SpaceObjectEnum spaceObjectType, int value)
        {
            Contract.Assert(spaceObjectType != SpaceObjectEnum.Stream && spaceObjectType != SpaceObjectEnum.Source);

            this.PermissionAssignableObject = permissionAssignableObject;
            this.SpaceObjectType = spaceObjectType;
            this.Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Permission"/> class.
        /// </summary>
        /// <param name="permissionAssignableObject">Permission assignable space object.</param>
        /// <param name="spaceObjectType">Space object type.</param>
        /// <param name="value">Permission value.</param>
        /// <param name="spaceObject">Space object.</param>
        public Permission(PermissionAssignableObject permissionAssignableObject, SpaceObjectEnum spaceObjectType, int value, SpaceObject spaceObject)
        {
            Contract.Assert(spaceObjectType == SpaceObjectEnum.Stream || spaceObjectType == SpaceObjectEnum.Source);

            this.PermissionAssignableObject = permissionAssignableObject;
            this.SpaceObjectType = spaceObjectType;
            this.Value = value;
            this.SpaceObject = spaceObject;
        }

        /// <summary>
        /// Gets or sets the permission value.
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// Gets the space object.
        /// </summary>
        public SpaceObject SpaceObject { get; private set; }

        /// <summary>
        /// Gets the space object type.
        /// </summary>
        public SpaceObjectEnum SpaceObjectType { get; private set; }

        /// <summary>
        /// Gets the permission assignable object type.
        /// </summary>
        public PermissionAssignableObject PermissionAssignableObject { get; private set; }
    }
}
