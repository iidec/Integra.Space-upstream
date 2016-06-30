//-----------------------------------------------------------------------
// <copyright file="VerifyExistence.cs" company="Integra.Space.Language">
//     Copyright (c) Integra.Space.Language. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Language.Pipeline.Filters
{
    using System;
    using CommandContext;

    /// <summary>
    /// Class to verify existence of the specified space objects.
    /// </summary>
    internal class VerifyExistence : Filter<PipelineCommandContext, PipelineCommandContext>
    {
        /// <inheritdoc />
        public override PipelineCommandContext Execute(PipelineCommandContext input)
        {
            throw new NotImplementedException();
        }
    }
}
