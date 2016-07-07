//-----------------------------------------------------------------------
// <copyright file="PipelineCommandContext.cs" company="Integra.Space.common">
//     Copyright (c) Integra.Space.Language. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Common.CommandContext
{
    using System.Diagnostics.Contracts;
    using Ninject;

    /// <summary>
    /// Command context class.
    /// </summary>
    internal class PipelineCommandContext
    {
        /// <summary>
        /// Space command.
        /// </summary>
        private SpaceCommand command;

        /// <summary>
        /// Kernel for dependency injection.
        /// </summary>
        private IKernel kernel;

        /// <summary>
        /// Initializes a new instance of the <see cref="PipelineCommandContext"/> class.
        /// </summary>
        /// <param name="command">Space command.</param>
        /// <param name="kernel">Kernel for dependency injection.</param>
        public PipelineCommandContext(SpaceCommand command, IKernel kernel)
        {
            Contract.Assert(command != null);
            Contract.Assert(kernel != null);

            this.command = command;
            this.kernel = kernel;
        }
        
        /// <summary>
        /// Gets the space command.
        /// </summary>
        public SpaceCommand Command
        {
            get
            {
                return this.command;
            }
        }

        /// <summary>
        /// Gets the kernel for dependency injection.
        /// </summary>
        public IKernel Kernel
        {
            get
            {
                return this.kernel;
            }
        }
    }
}
