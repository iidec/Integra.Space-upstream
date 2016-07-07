//-----------------------------------------------------------------------
// <copyright file="CommandPipelineBuilder.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline
{
    using Common.CommandContext;

    /// <summary>
    /// Command pipeline builder.
    /// </summary>
    internal class CommandPipelineBuilder
    {
        /// <summary>
        /// Build the pipeline.
        /// </summary>
        /// <returns>The pipeline to execute.</returns>
        public Filter<string, ExecutionPipelineNode<PipelineCommandContext, PipelineCommandContext>> Build()
        {
            Filter<string, ExecutionPipelineNode<PipelineCommandContext, PipelineCommandContext>> result =
                new Filters.FilterCommandParser()
                .AddStep(new Filters.SpecificFiltersAggregator())
                .AddStep(new Filters.CommonFiltersAggregator());

            return result;
        }
    }
}
