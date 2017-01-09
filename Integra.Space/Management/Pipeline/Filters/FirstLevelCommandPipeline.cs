//-----------------------------------------------------------------------
// <copyright file="FirstLevelCommandPipeline.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline
{
    using Integra.Space.Pipeline;

    /// <summary>
    /// Command pipeline class.
    /// </summary>
    internal class FirstLevelCommandPipeline : Pipeline<FirstLevelPipelineContext, FirstLevelPipelineContext, FirstLevelPipelineContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FirstLevelCommandPipeline"/> class.
        /// </summary>
        /// <param name="source">The source of inputs.</param>
        /// <param name="destination">The destination of outputs.</param>
        public FirstLevelCommandPipeline(Filter<FirstLevelPipelineContext, FirstLevelPipelineContext> source, Filter<FirstLevelPipelineContext, FirstLevelPipelineContext> destination) : base(source, destination)
        {
        }
    }
}
