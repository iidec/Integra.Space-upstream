//-----------------------------------------------------------------------
// <copyright file="FilterLock.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Common;

    /// <summary>
    /// Filter lock class.
    /// </summary>
    internal sealed class FilterLock : CommandFilter
    {
        /// <inheritdoc />
        public override PipelineExecutionCommandContext Execute(PipelineExecutionCommandContext input)
        {
            foreach (SpaceObjectEnum o in input.Command.GetUsedSpaceObjectTypes())
            {
                /* aqui se bloquearian cada uno de los tipos de objetos */
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