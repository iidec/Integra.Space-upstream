//-----------------------------------------------------------------------
// <copyright file="PipelineContext.cs" company="Integra.Space.common">
//     Copyright (c) Integra.Space.Language. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline
{
    using System.Diagnostics.Contracts;
    using Language;
    using Models;
    using Ninject;

    /// <summary>
    /// Command context class.
    /// </summary>
    internal class PipelineContext
    {
        /// <summary>
        /// Space command string.
        /// </summary>
        private string commandString;

        /// <summary>
        /// Kernel for dependency injection.
        /// </summary>
        private IKernel kernel;

        /// <summary>
        /// Space command.
        /// </summary>
        private SystemCommand command;

        /// <summary>
        /// Error thrown in the pipeline.
        /// </summary>
        private System.Exception error;

        /// <summary>
        /// Created pipeline.
        /// </summary>
        private Filter<PipelineContext, PipelineContext> pipeline;

        /// <summary>
        /// Flag indicating that the user is allowed to do things to a specific object
        /// </summary>
        private bool? isAllowedOverSpecificObject;

        /// <summary>
        /// Flag indicating that the user is allowed to do things in a global environment. Example. create a schema, role, or user.
        /// </summary>
        private bool? isAllowedOverSystem;

        /// <summary>
        /// Initializes a new instance of the <see cref="PipelineContext"/> class.
        /// </summary>
        /// <param name="commandString">Space command string.</param>
        /// <param name="user">The user requesting the command execution.</param>
        /// <param name="kernel">Kernel for dependency injection.</param>
        public PipelineContext(string commandString, User user, IKernel kernel)
        {
            Contract.Assert(!string.IsNullOrWhiteSpace(commandString));
            Contract.Assert(user != null);
            Contract.Assert(kernel != null);

            this.commandString = commandString;
            this.User = user;
            this.kernel = kernel;
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
        /// Gets the user requesting the command execution.
        /// </summary>
        public User User { get; private set; }

        /// <summary>
        /// Gets the space command string.
        /// </summary>
        public string CommandString
        {
            get
            {
                return this.commandString;
            }
        }

        /// <summary>
        /// Gets or sets the space command.
        /// </summary>
        public SystemCommand Command
        {
            get
            {
                return this.command;
            }

            set
            {
                if (this.command == null)
                {
                    this.command = value;
                }
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
        /// Gets or sets the created pipeline.
        /// </summary>
        public Filter<PipelineContext, PipelineContext> Pipeline
        {
            get
            {
                return this.pipeline;
            }

            set
            {
                this.pipeline = value;
            }
        }

        /// <summary>
        /// Sets a value indicating that the user is allowed to do things to a specific object
        /// </summary>
        public bool? IsAllowedOverSpecificObject
        {
            set
            {
                if (this.isAllowedOverSpecificObject == null)
                {
                    this.isAllowedOverSpecificObject = value;
                }
            }
        }

        /// <summary>
        /// Sets a value indicating that the user is allowed to do things in a global environment. Example. create a schema, role, or user.
        /// </summary>
        public bool? IsAllowedOverSystem
        {
            set
            {
                if (this.isAllowedOverSystem == null)
                {
                    this.isAllowedOverSystem = value;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the user has permissions to execute the command.
        /// </summary>
        /// <returns>A value indicating whether the user has permissions to execute the command.</returns>
        public bool HasPermissions()
        {
            if (this.isAllowedOverSpecificObject == null && this.isAllowedOverSystem == null)
            {
                return false;
            }
            else if (this.isAllowedOverSpecificObject == false || this.isAllowedOverSystem == false)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
