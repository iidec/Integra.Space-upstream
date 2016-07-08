//-----------------------------------------------------------------------
// <copyright file="StreamAssembly.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Models
{
    using System.Reflection;

    /// <summary>
    /// Stream assembly class.
    /// </summary>
    internal class StreamAssembly
    {
        /// <summary>
        /// Assembly of the query.
        /// </summary>
        private Assembly assembly;

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamAssembly"/> class.
        /// </summary>
        /// <param name="assembly">Assembly of the query.</param>
        public StreamAssembly(Assembly assembly)
        {
            this.assembly = assembly;
        }

        /// <summary>
        /// Gets the assembly of the query.
        /// </summary>
        public Assembly Assembly
        {
            get
            {
                return this.assembly;
            }
        }

        /// <summary>
        /// Gets or sets the path to the assembly.
        /// </summary>
        public string PathToAssembly { get; set; }
    }
}
