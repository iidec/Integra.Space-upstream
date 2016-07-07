//-----------------------------------------------------------------------
// <copyright file="ExecutionPipelineNode.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline
{
    using System.Diagnostics.Contracts;
    using Common;
    using Common.CommandContext;

    /// <summary>
    /// Execution pipeline node class.
    /// </summary>
    /// <typeparam name="TPipelineInput">Input type of the execution pipeline.</typeparam>
    /// <typeparam name="TPipelineOutput">Output type of the execution pipeline.</typeparam>
    internal class ExecutionPipelineNode<TPipelineInput, TPipelineOutput>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionPipelineNode{TPipelineInput, TPipelineOutput}"/> class.
        /// </summary>
        /// <param name="pipeline">Execution pipeline created.</param>
        /// <param name="command">Command to execute.</param>
        public ExecutionPipelineNode(Filter<TPipelineInput, TPipelineOutput> pipeline, SpaceCommand command)
        {
            Contract.Assert(pipeline != null);
            Contract.Assert(command != null);

            this.Pipeline = pipeline;
            this.Command = command;
        }

        /// <summary>
        /// Gets the execution pipeline created until now
        /// </summary>
        public Filter<TPipelineInput, TPipelineOutput> Pipeline { get; private set; }

        /// <summary>
        /// Gets the space command to execute.
        /// </summary>
        public SpaceCommand Command { get; private set; }
    }
}
