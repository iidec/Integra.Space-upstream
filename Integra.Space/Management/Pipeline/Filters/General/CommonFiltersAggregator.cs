﻿//-----------------------------------------------------------------------
// <copyright file="CommonFiltersAggregator.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;

    /// <summary>
    /// Common filters aggregator class.
    /// </summary>
    internal class CommonFiltersAggregator : CommandFilter
    {
        /// <inheritdoc />
        public override PipelineContext Execute(PipelineContext input)
        {
            input.Pipeline = new FilterLock()
                .AddStep(new ValidateExistence())
                .AddStep(new ValidateSpecificObjectPermissions())
                .AddStep(new ValidateObjectTypePermissions())
                .AddStep(input.Pipeline)
                .AddStep(new FilterUnlock());

            return input;
        }

        /// <inheritdoc />
        public override void OnError(PipelineContext e)
        {
            throw new NotImplementedException();
        }
    }
}
