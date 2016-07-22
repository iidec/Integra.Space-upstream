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
    internal class Stream : SecureObject
    {
        /// <summary>
        /// Assembly of the query of the stream.
        /// </summary>
        private SystemAssembly streamAssembly;

        /// <summary>
        /// Initializes a new instance of the <see cref="Stream"/> class.
        /// </summary>
        /// <param name="guid">Space object unique identifier.</param>
        /// <param name="identifier">Space object name.</param>
        /// <param name="query">Query of the stream.</param>
        /// <param name="schema">Schema witch the secure object belongs.</param>
        public Stream(System.Guid guid, string identifier, string query, Schema schema) : base(guid, identifier, schema)
        {
            this.Query = query;
        }

        /// <summary>
        /// Gets or sets the query of the stream.
        /// </summary>
        public string Query { get; set; }

        /// <summary>
        /// Gets or sets the assembly of the query of the stream.
        /// </summary>
        public SystemAssembly StreamAssembly
        {
            get
            {
                return this.StreamAssembly;
            }

            set
            {
                if (this.streamAssembly != null)
                {
                    this.streamAssembly = value;
                }
            }
        }
    }
}
