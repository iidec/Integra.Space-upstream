//-----------------------------------------------------------------------
// <copyright file="PipelineExecutionCommandContext.cs" company="Integra.Space.common">
//     Copyright (c) Integra.Space.Language. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline
{
    using System.Diagnostics.Contracts;
    using Common;
    using Models;
    using Ninject;

    /// <summary>
    /// Command context class.
    /// </summary>
    internal class PipelineExecutionCommandContext
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
        /// Error thrown in the pipeline.
        /// </summary>
        private System.Exception error;

        /// <summary>
        /// Initializes a new instance of the <see cref="PipelineExecutionCommandContext"/> class.
        /// </summary>
        /// <param name="command">Space command.</param>
        /// <param name="user">The user requesting the command execution.</param>
        /// <param name="kernel">Kernel for dependency injection.</param>
        public PipelineExecutionCommandContext(SpaceCommand command, User user, IKernel kernel)
        {
            Contract.Assert(command != null);
            Contract.Assert(user != null);
            Contract.Assert(kernel != null);

            this.command = command;
            this.User = user;
            this.kernel = kernel;
        }

        /// <summary>
        /// Gets the user requesting the command execution.
        /// </summary>
        public User User { get; private set; }

        /// <summary>
        /// Gets or sets the space command.
        /// </summary>
        public SpaceCommand Command
        {
            get
            {
                return this.command;
            }

            set
            {
                if (value != null)
                {
                    this.command = value;
                }
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

        /// <summary>
        /// Gets or sets the error thrown in the pipeline.
        /// </summary>
        public System.Exception Error
        {
            get
            {
                return this.error;
            }

            set
            {
                if (this.error == null)
                {
                    this.error = value;
                }
            }
        }
    }
}
