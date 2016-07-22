//-----------------------------------------------------------------------
// <copyright file="SecureObject.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Models
{
    using System.Collections.Generic;

    /// <summary>
    /// Space object class.
    /// </summary>
    internal abstract class SecureObject : SystemObject
    {
        /// <summary>
        /// Permissions assigned to the secure object.
        /// </summary>
        private ICollection<PermissionAssigned> permissions;

        /// <summary>
        /// Initializes a new instance of the <see cref="SecureObject"/> class.
        /// </summary>
        /// <param name="guid">Space object identifier.</param>
        /// <param name="name">Space object name.</param>
        /// <param name="schema">Schema witch the secure object belongs.</param>
        public SecureObject(System.Guid guid, string name, Schema schema) : base(guid, name)
        {
            this.Schema = schema;
            this.permissions = new List<PermissionAssigned>();
        }

        /// <summary>
        /// Gets the schema to which the object belongs.
        /// </summary>
        public Schema Schema { get; private set; }

        /// <summary>
        /// Gets the permissions assigned to the secure object.
        /// </summary>
        public ICollection<PermissionAssigned> Permissions
        {
            get
            {
                return this.permissions;
            }
        }
    }
}
