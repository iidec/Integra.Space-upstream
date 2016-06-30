//-----------------------------------------------------------------------
// <copyright file="ValidateExistence.cs" company="Integra.Space.Language">
//     Copyright (c) Integra.Space.Language. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Language.Pipeline.Filters
{
    using System;
    using CommandContext;

    /// <summary>
    /// Class to validate whether the existence of the specified space objects applies to the command.
    /// </summary>
    internal class ValidateExistence : Filter<PipelineCommandContext, PipelineCommandContext>
    {
        /// <inheritdoc />
        public override PipelineCommandContext Execute(PipelineCommandContext input)
        {
            foreach (Tuple<string, SpaceObjectEnum, bool?> objects in input.SpaceObjects)
            {
                bool? exist = false;
                /* verificar si existe el objecto */                
            }

            return input;
        }
    }
}
