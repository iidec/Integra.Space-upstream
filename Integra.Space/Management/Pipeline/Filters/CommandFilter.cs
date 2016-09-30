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
    internal abstract class CommandFilter : Filter<PipelineContext, PipelineContext>
    {
        /// <inheritdoc />
        public override PipelineContext Execute(PipelineContext context)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override void OnError(PipelineContext context)
        {
            throw new NotImplementedException();
        }
    }
}
