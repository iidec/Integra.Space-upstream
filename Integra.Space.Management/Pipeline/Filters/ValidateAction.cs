//-----------------------------------------------------------------------
// <copyright file="ValidateAction.cs" company="Integra.Space.Language">
//     Copyright (c) Integra.Space.Language. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Language.Pipeline.Filters
{
    using System;
    using CommandContext;

    /// <summary>
    /// Class to validate whether the action applies to the specified space object.
    /// </summary>
    internal class ValidateAction : Filter<PipelineCommandContext, PipelineCommandContext>
    {
        /// <inheritdoc />
        public override PipelineCommandContext Execute(PipelineCommandContext input)
        {
            SpaceActionCommandEnum action = input.Action;
            
            return input;
        }
    }
}
