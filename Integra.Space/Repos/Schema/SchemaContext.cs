//-----------------------------------------------------------------------
// <copyright file="SchemaContext.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Cache
{
    using System.Collections.Generic;
    using Models;
    
    /// <summary>
    /// Cache context class.
    /// </summary>
    internal class SchemaContext
    {
        /// <summary>
        /// Stream repository.
        /// </summary>
        private List<Stream> streams = new List<Stream>();

        /// <summary>
        /// Source repository.
        /// </summary>
        private List<Source> sources = new List<Source>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaContext"/> class.
        /// </summary>
        public SchemaContext()
        {
            this.sources = new List<Source>();
            this.streams = new List<Stream>();
        }

        /// <summary>
        /// Gets the stream repository.
        /// </summary>
        public List<Stream> Streams
        {
            get
            {
                return this.streams;
            }
        }

        /// <summary>
        /// Gets the source repository.
        /// </summary>
        public List<Source> Sources
        {
            get
            {
                return this.sources;
            }
        }
    }
}
