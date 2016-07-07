//-----------------------------------------------------------------------
// <copyright file="SpaceObject.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Models
{
    /// <summary>
    /// Space object class.
    /// </summary>
    internal abstract class SpaceObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpaceObject"/> class.
        /// </summary>
        /// <param name="guid">Space object identifier.</param>
        /// <param name="name">Space object name.</param>
        public SpaceObject(System.Guid guid, string name)
        {
            this.Guid = guid;
            this.Identifier = name;
        }

        /// <summary>
        /// Gets the identifier of the space object.
        /// </summary>
        public string Identifier { get; private set; }

        /// <summary>
        /// Gets the unique identifier of the object.
        /// </summary>
        public System.Guid Guid { get; private set; }
    }
}
