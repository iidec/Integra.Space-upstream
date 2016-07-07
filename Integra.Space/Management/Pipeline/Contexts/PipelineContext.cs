//-----------------------------------------------------------------------
// <copyright file="PipelineContext.cs" company="Integra.Space.common">
//     Copyright (c) Integra.Space.Language. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline
{
    using System.Diagnostics.Contracts;
    using Common;

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
        /// Space command.
        /// </summary>
        private SpaceCommand command;

        /// <summary>
        /// Error thrown in the pipeline.
        /// </summary>
        private System.Exception error;

        /// <summary>
        /// Created pipeline.
        /// </summary>
        private Filter<PipelineExecutionCommandContext, PipelineExecutionCommandContext> pipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="PipelineContext"/> class.
        /// </summary>
        /// <param name="commandString">Space command string.</param>
        public PipelineContext(string commandString)
        {
            Contract.Assert(!string.IsNullOrWhiteSpace(commandString));

            this.commandString = commandString;
        }

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
        public SpaceCommand Command
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
        public Filter<PipelineExecutionCommandContext, PipelineExecutionCommandContext> Pipeline
        {
            get
            {
                return this.pipeline;
            }

            set
            {
                if (this.pipeline == null)
                {
                    this.pipeline = value;
                }
            }
        }
    }
}
