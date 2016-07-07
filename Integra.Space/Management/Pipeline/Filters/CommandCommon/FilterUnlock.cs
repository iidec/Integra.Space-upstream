//-----------------------------------------------------------------------
// <copyright file="FilterUnlock.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Common;
    using Common.CommandContext;

    /// <summary>
    /// Filter lock class.
    /// </summary>
    internal sealed class FilterUnlock : Filter<PipelineCommandContext, PipelineCommandContext>
    {
        /// <inheritdoc />
        public override PipelineCommandContext Execute(PipelineCommandContext input)
        {
            foreach (SpaceObjectEnum o in input.Command.GetUsedSpaceObjectTypes())
            {
                /* aqui se bloquearian cada uno de los tipos de objetos */
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