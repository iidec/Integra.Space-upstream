//-----------------------------------------------------------------------
// <copyright file="FirstLevelCommandFilter.cs" company="Integra.Space">
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
    internal abstract class FirstLevelCommandFilter : Filter<FirstLevelPipelineContext, FirstLevelPipelineContext>
    {
        /// <inheritdoc />
        public override FirstLevelPipelineContext Execute(FirstLevelPipelineContext context)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override void OnError(FirstLevelPipelineContext context)
        {
            throw new NotImplementedException();
        }
    }
}
