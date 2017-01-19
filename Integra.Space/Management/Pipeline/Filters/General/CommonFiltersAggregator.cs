//-----------------------------------------------------------------------
// <copyright file="CommonFiltersAggregator.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Common filters aggregator class.
    /// </summary>
    internal class CommonFiltersAggregator : FirstLevelCommandFilter
    {
        /// <inheritdoc />
        public override FirstLevelPipelineContext Execute(FirstLevelPipelineContext context)
        {
            Filter<PipelineContext, PipelineContext> filterAux = null;
            foreach (CommandPipelineNode commandNode in context.Commands)
            {
                if (commandNode.Command is Language.QueryCommandForMetadataNode)
                {
                    filterAux = new ValidateExistence();
                }
                else
                {
                    filterAux = new ValidateExistence()
                        .AddStep(new ValidatePermissions());
                }

                if (commandNode.Pipeline != null)
                {
                    commandNode.Pipeline = filterAux.AddStep(commandNode.Pipeline);
                }
            }

            return context;
        }

        /// <inheritdoc />
        public override void OnError(FirstLevelPipelineContext e)
        {
            throw new NotImplementedException();
        }
    }
}
