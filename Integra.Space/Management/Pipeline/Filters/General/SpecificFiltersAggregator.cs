//-----------------------------------------------------------------------
// <copyright file="SpecificFiltersAggregator.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using System.Collections.Generic;
    using Common;

    /// <summary>
    /// Specific filters aggregator class.
    /// </summary>
    internal class SpecificFiltersAggregator : FirstPipelineFilter
    {
        /// <inheritdoc />
        public override PipelineContext Execute(PipelineContext context)
        {
            context.Pipeline = SpecificFilterSelector.GetSpecificFilter(new SpecificFilterKey(context.Command.Action, context.Command.SpaceObjectType));
            
            return context;
        }

        /// <inheritdoc />
        public override void OnError(PipelineContext e)
        {
            throw new NotImplementedException();
        }        
    }
}
