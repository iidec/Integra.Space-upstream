//-----------------------------------------------------------------------
// <copyright file="GenericPipelineExecutor.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline
{
    /// <summary>
    /// Command pipeline builder.
    /// </summary>
    internal class GenericPipelineExecutor : PipelineExecutor<PipelineContext, PipelineContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenericPipelineExecutor"/> class.
        /// </summary>
        /// <param name="pipeline">Pipeline to execute.</param>
        public GenericPipelineExecutor(Filter<PipelineContext, PipelineContext> pipeline) : base(pipeline)
        {
        }
    }
}
