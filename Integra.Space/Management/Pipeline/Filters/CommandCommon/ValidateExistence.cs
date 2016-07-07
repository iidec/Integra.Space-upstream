//-----------------------------------------------------------------------
// <copyright file="ValidateExistence.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using Common;

    /// <summary>
    /// Class to validate whether the existence of the specified space objects applies to the command.
    /// </summary>
    internal class ValidateExistence : CommandFilter
    {
        /// <inheritdoc />
        public override PipelineExecutionCommandContext Execute(PipelineExecutionCommandContext input)
        {
            foreach (Tuple<SpaceObjectEnum, string> objects in input.Command.GetUsedSpaceObjects())
            {
                bool? exist = false;
                /* verificar si existe el objecto */                
            }

            return input;
        }

        /// <inheritdoc />
        public override void OnError(PipelineExecutionCommandContext e)
        {
            throw new NotImplementedException();
        }
    }
}
