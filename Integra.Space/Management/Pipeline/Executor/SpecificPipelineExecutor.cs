//-----------------------------------------------------------------------
// <copyright file="SpecificPipelineExecutor.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline
{
    /// <summary>
    /// Command pipeline builder.
    /// </summary>
    internal class SpecificPipelineExecutor : PipelineExecutor<PipelineExecutionCommandContext, PipelineExecutionCommandContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpecificPipelineExecutor"/> class.
        /// </summary>
        /// <param name="pipeline">Pipeline to execute.</param>
        public SpecificPipelineExecutor(Filter<PipelineExecutionCommandContext, PipelineExecutionCommandContext> pipeline) : base(pipeline)
        {
        }
    }
}
