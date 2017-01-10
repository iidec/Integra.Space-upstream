//-----------------------------------------------------------------------
// <copyright file="PipelineContext.cs" company="Integra.Space.common">
//     Copyright (c) Integra.Space.Language. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline
{
    using System.Diagnostics.Contracts;
    using System.Reflection.Emit;
    using Language;
    using Ninject;

    /// <summary>
    /// Command context class.
    /// </summary>
    internal class PipelineContext
    {
        /// <summary>
        /// Kernel for dependency injection.
        /// </summary>
        private IKernel kernel;

        /// <summary>
        /// Error thrown in the pipeline.
        /// </summary>
        private System.Exception error;

        /// <summary>
        /// Assembly builder.
        /// </summary>
        private AssemblyBuilder assemblyBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="PipelineContext"/> class.
        /// </summary>
        /// <param name="commandString">Space command string.</param>
        /// <param name="command">Compiled command.</param>
        /// <param name="login">The login requesting the command execution.</param>
        /// <param name="kernel">Kernel for dependency injection.</param>
        public PipelineContext(string commandString, SystemCommand command, string login, IKernel kernel)
        {
            Contract.Assert(!string.IsNullOrWhiteSpace(commandString));
            Contract.Assert(login != null);
            Contract.Assert(kernel != null);
            
            this.kernel = kernel;
            
            this.SecurityContext = new SecurityContext(login, kernel);
            this.CommandContext = new CommandContext(commandString, command, this.SecurityContext.Login, kernel);
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
        /// Gets the command context.
        /// </summary>
        public CommandContext CommandContext { get; private set; }

        /// <summary>
        /// Gets the security context for the command execution.
        /// </summary>
        public SecurityContext SecurityContext { get; private set; }
        
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

        /// <summary>
        /// Gets or sets the assembly builder for the current command.
        /// </summary>
        public AssemblyBuilder AssemblyBuilder
        {
            get
            {
                return this.assemblyBuilder;
            }

            set
            {
                if (this.assemblyBuilder == null)
                {
                    this.assemblyBuilder = value;
                }
            }
        }
    }
}
