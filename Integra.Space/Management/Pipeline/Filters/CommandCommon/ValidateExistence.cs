//-----------------------------------------------------------------------
// <copyright file="ValidateExistence.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using System.Collections.Generic;
    using Common;
    using Common.CommandContext;

    /// <summary>
    /// Class to validate whether the existence of the specified space objects applies to the command.
    /// </summary>
    internal class ValidateExistence : Filter<PipelineCommandContext, PipelineCommandContext>
    {
        /// <inheritdoc />
        public override PipelineCommandContext Execute(PipelineCommandContext input)
        {
            foreach (Tuple<SpaceObjectEnum, string> objects in input.Command.GetUsedSpaceObjects())
            {
                bool? exist = false;
                /* verificar si existe el objecto */                
            }

            return input;
        }

        /// <inheritdoc />
        public override void OnError(Exception e)
        {
            throw new NotImplementedException();
        }
    }
}
