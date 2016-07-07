//-----------------------------------------------------------------------
// <copyright file="FirstPipeline.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline
{
    using Integra.Space.Pipeline;

    /// <summary>
    /// Command pipeline class.
    /// </summary>
    internal class FirstPipeline : Pipeline<PipelineContext, PipelineContext, PipelineContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FirstPipeline"/> class.
        /// </summary>
        /// <param name="source">The source of inputs.</param>
        /// <param name="destination">The destination of outputs.</param>
        public FirstPipeline(Filter<PipelineContext, PipelineContext> source, Filter<PipelineContext, PipelineContext> destination) : base(source, destination)
        {
        }
    }
}
