//-----------------------------------------------------------------------
// <copyright file="CommonFiltersAggregator.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using Common.CommandContext;

    /// <summary>
    /// Common filters aggregator class.
    /// </summary>
    internal class CommonFiltersAggregator : Filter<ExecutionPipelineNode<PipelineCommandContext, PipelineCommandContext>, ExecutionPipelineNode<PipelineCommandContext, PipelineCommandContext>>
    {
        /// <inheritdoc />
        public override ExecutionPipelineNode<PipelineCommandContext, PipelineCommandContext> Execute(ExecutionPipelineNode<PipelineCommandContext, PipelineCommandContext> input)
        {
            Filter<PipelineCommandContext, PipelineCommandContext> pipeline = new Filters.FilterLock()
                .AddStep(new ValidatePermissions())
                .AddStep(new VerifyExistence())
                .AddStep(new ValidateExistence())
                .AddStep(input.Pipeline)
                .AddStep(new FilterUnlock());

            return new ExecutionPipelineNode<PipelineCommandContext, PipelineCommandContext>(pipeline, input.Command);
        }

        /// <inheritdoc />
        public override void OnError(Exception e)
        {
            throw new NotImplementedException();
        }
    }
}
