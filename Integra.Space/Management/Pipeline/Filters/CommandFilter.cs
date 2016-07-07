//-----------------------------------------------------------------------
// <copyright file="CommandFilter.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using Integra.Space.Pipeline;

    /// <summary>
    /// Command filter.
    /// </summary>
    internal abstract class CommandFilter : Filter<PipelineExecutionCommandContext, PipelineExecutionCommandContext>
    {
        /// <inheritdoc />
        public override PipelineExecutionCommandContext Execute(PipelineExecutionCommandContext context)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override void OnError(PipelineExecutionCommandContext context)
        {
            if (!this.Executed)
            {
                return;
            }
        }
    }
}
