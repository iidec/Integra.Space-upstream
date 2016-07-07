//-----------------------------------------------------------------------
// <copyright file="CommandPipelineBuilder.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline
{
    /// <summary>
    /// Command pipeline builder.
    /// </summary>
    internal class CommandPipelineBuilder
    {
        /// <summary>
        /// Build the pipeline.
        /// </summary>
        /// <returns>The pipeline to execute.</returns>
        public Filter<PipelineContext, PipelineContext> Build()
        {
            Filter<PipelineContext, PipelineContext> result =
                new Filters.FilterCommandParser()
                .AddStep(new Filters.SpecificFiltersAggregator())
                .AddStep(new Filters.CommonFiltersAggregator());

            return result;
        }
    }
}
