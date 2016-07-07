//-----------------------------------------------------------------------
// <copyright file="CommandPipeline.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline
{
    using Integra.Space.Pipeline;

    /// <summary>
    /// Command pipeline class.
    /// </summary>
    internal class CommandPipeline : Pipeline<PipelineExecutionCommandContext, PipelineExecutionCommandContext, PipelineExecutionCommandContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandPipeline"/> class.
        /// </summary>
        /// <param name="source">The source of inputs.</param>
        /// <param name="destination">The destination of outputs.</param>
        public CommandPipeline(Filter<PipelineExecutionCommandContext, PipelineExecutionCommandContext> source, Filter<PipelineExecutionCommandContext, PipelineExecutionCommandContext> destination) : base(source, destination)
        {
        }
    }
}
