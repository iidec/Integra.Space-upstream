//-----------------------------------------------------------------------
// <copyright file="FilterUnlock.cs" company="Integra.Space.Language">
//     Copyright (c) Integra.Space.Language. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Language.Pipeline.Filters
{
    using System.Collections.Generic;
    using System.Linq;
    using CommandContext;

    /// <summary>
    /// Filter lock class.
    /// </summary>
    internal sealed class FilterUnlock : Filter<PipelineCommandContext, PipelineCommandContext>
    {
        /// <inheritdoc />
        public override PipelineCommandContext Execute(PipelineCommandContext input)
        {
            IEnumerable<IGrouping<SpaceObjectEnum, SpaceObjectEnum>> objects = input.SpaceObjects.GroupBy(x => x.Item2, x => x.Item2);

            foreach (IGrouping<SpaceObjectEnum, SpaceObjectEnum> o in objects)
            {
                SpaceObjectEnum objectToBlock = o.Key;

                /* aqui se bloquearian cada uno de los tipos de objetos */
            }

            return input;
        }
    }
}