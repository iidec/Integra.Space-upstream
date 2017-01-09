//-----------------------------------------------------------------------
// <copyright file="CommandPipelineNode.cs" company="Integra.Space.common">
//     Copyright (c) Integra.Space.Language. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline
{
    using Integra.Space.Language;

    /// <summary>
    /// Command pipeline node.
    /// </summary>
    internal class CommandPipelineNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandPipelineNode"/> class.
        /// </summary>
        /// <param name="command">System command that will be executed.</param>
        public CommandPipelineNode(SystemCommand command)
        {
            this.Command = command;
        }

        /// <summary>
        /// Gets the command to execute.
        /// </summary>
        public SystemCommand Command { get; private set; }

        /// <summary>
        /// Gets or sets the pipeline of the command.
        /// </summary>
        public Filter<PipelineContext, PipelineContext> Pipeline { get; set; }
    }
}
