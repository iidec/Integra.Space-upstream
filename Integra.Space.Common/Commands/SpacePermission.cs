//-----------------------------------------------------------------------
// <copyright file="SpacePermission.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Common
{
    /// <summary>
    /// Space permission class.
    /// </summary>
    internal class SpacePermission
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpacePermission"/> class.
        /// </summary>
        /// <param name="permission">Space permission.</param>
        /// <param name="objectType">Space objet type.</param>
        /// <param name="objectName">Object name.</param>
        public SpacePermission(SpacePermissionsEnum permission, SpaceObjectEnum objectType, string objectName)
        {
            this.Permission = permission;
            this.ObjectType = objectType;
            this.ObjectName = objectName;
        }

        /// <summary>
        /// Gets the permission.
        /// </summary>
        public SpacePermissionsEnum Permission { get; private set; }

        /// <summary>
        /// Gets the object type.
        /// </summary>
        public SpaceObjectEnum ObjectType { get; private set; }

        /// <summary>
        /// Gets the object name.
        /// </summary>
        public string ObjectName { get; private set; }
    }
}
