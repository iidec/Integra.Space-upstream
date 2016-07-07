//-----------------------------------------------------------------------
// <copyright file="Stream.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Models
{
    /// <summary>
    /// Space object class.
    /// </summary>
    internal class Stream : SpaceObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Stream"/> class.
        /// </summary>
        /// <param name="guid">Space object unique identifier.</param>
        /// <param name="identifier">Space object name.</param>
        /// <param name="query">Query of the stream.</param>
        public Stream(System.Guid guid, string identifier, string query) : base(guid, identifier)
        {
            this.Query = query;
        }

        /// <summary>
        /// Gets the query of the stream.
        /// </summary>
        public string Query { get; private set; }
    }
}
