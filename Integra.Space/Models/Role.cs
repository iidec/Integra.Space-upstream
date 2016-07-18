//-----------------------------------------------------------------------
// <copyright file="Role.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Models
{
    using System.Collections.Generic;
    using Integra.Space.Common;

    /// <summary>
    /// Space object class.
    /// </summary>
    internal class Role : PermissionAssignableObject
    {
        /// <summary>
        /// Role type enumerator.
        /// </summary>
        private SpaceRoleTypeEnum roleType;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Role"/> class.
        /// </summary>
        /// <param name="guid">Space object unique identifier.</param>
        /// <param name="identifier">Space object name.</param>
        /// <param name="roleType">Role type.</param>
        public Role(System.Guid guid, string identifier, SpaceRoleTypeEnum roleType) : base(guid, identifier)
        {
            this.roleType = roleType;
        }

        /// <summary>
        /// Gets the role type of the role.
        /// </summary>
        public SpaceRoleTypeEnum RoleType
        {
            get
            {
                return this.roleType;
            }
        }
    }
}
