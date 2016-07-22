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
    internal class Source : SecureObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Source"/> class.
        /// </summary>
        /// <param name="guid">Space object unique identifier.</param>
        /// <param name="identifier">Space object name.</param>
        /// <param name="schema">Schema witch the secure object belongs.</param>
        public Source(System.Guid guid, string identifier, Schema schema) : base(guid, identifier, schema)
        {
        }
    }
}
