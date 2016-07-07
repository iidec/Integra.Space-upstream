//-----------------------------------------------------------------------
// <copyright file="Source.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Models
{
    /// <summary>
    /// Space source object class.
    /// </summary>
    internal class Source : SpaceObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Source"/> class.
        /// </summary>
        /// <param name="guid">Space object unique identifier.</param>
        /// <param name="identifier">Space object name.</param>
        public Source(System.Guid guid, string identifier) : base(guid, identifier)
        {
        }
    }
}
