//-----------------------------------------------------------------------
// <copyright file="VerifyExistence.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using Common;
    using Common.CommandContext;

    /// <summary>
    /// Class to verify existence of the specified space objects.
    /// </summary>
    internal class VerifyExistence : Filter<PipelineCommandContext, PipelineCommandContext>
    {
        /// <inheritdoc />
        public override PipelineCommandContext Execute(PipelineCommandContext input)
        {
            return input;
        }
    }
}
