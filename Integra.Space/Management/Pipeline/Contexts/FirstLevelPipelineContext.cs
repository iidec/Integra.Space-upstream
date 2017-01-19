//-----------------------------------------------------------------------
// <copyright file="FirstLevelPipelineContext.cs" company="Integra.Space.common">
//     Copyright (c) Integra.Space.Language. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline
{
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using Ninject;

    /// <summary>
    /// Command context class.
    /// </summary>
    internal class FirstLevelPipelineContext
    {
        /// <summary>
        /// Login that request the execution.
        /// </summary>
        private string login;

        /// <summary>
        /// Kernel for dependency injection.
        /// </summary>
        private IKernel kernel;

        /// <summary>
        /// Error thrown in the pipeline.
        /// </summary>
        private System.Exception error;

        /// <summary>
        /// Array of commands.
        /// </summary>
        private List<CommandPipelineNode> commands;

        /// <summary>
        /// Created pipeline.
        /// </summary>
        private List<Filter<PipelineContext, PipelineContext>> pipeline;

        /// <summary>
        /// Batch string.
        /// </summary>
        private string batchString;

        /// <summary>
        /// Initializes a new instance of the <see cref="FirstLevelPipelineContext"/> class.
        /// </summary>
        /// <param name="batchString">Batch string.</param>
        /// <param name="login">The login requesting the command execution.</param>
        /// <param name="kernel">Kernel for dependency injection.</param>
        public FirstLevelPipelineContext(string batchString, string login, IKernel kernel)
        {
            Contract.Assert(!string.IsNullOrWhiteSpace(batchString));
            Contract.Assert(login != null);
            Contract.Assert(kernel != null);

            this.batchString = batchString;
            this.commands = new List<CommandPipelineNode>();
            this.pipeline = new List<Filter<PipelineContext, PipelineContext>>();
            this.login = login;
            this.kernel = kernel;
        }

        /// <summary>
        /// Gets the batch string.
        /// </summary>
        public string BatchString
        {
            get
            {
                return this.batchString;
            }
        }

        /// <summary>
        /// Gets the login that request the execution.
        /// </summary>
        public string Login
        {
            get
            {
                return this.login;
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

        /// <summary>
        /// Gets or sets the array of commands.
        /// </summary>
        public List<CommandPipelineNode> Commands
        {
            get
            {
                return this.commands;
            }

            set
            {
                if (this.commands == null)
                {
                    this.commands = value;
                }
            }
        }
    }
}
