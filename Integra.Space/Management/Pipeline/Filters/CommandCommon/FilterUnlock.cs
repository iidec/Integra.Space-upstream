//-----------------------------------------------------------------------
// <copyright file="FilterUnlock.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using Common;

    /// <summary>
    /// Filter lock class.
    /// </summary>
    internal sealed class FilterUnlock : CommandFilter
    {
        /// <inheritdoc />
        public override PipelineContext Execute(PipelineContext input)
        {
            foreach (SystemObjectEnum o in input.Command.GetUsedSpaceObjectTypes())
            {
                /* aqui se bloquearian cada uno de los tipos de objetos */
            }

            return input;
        }

        /// <inheritdoc />
        public override void OnError(PipelineContext e)
        {
            throw new NotImplementedException();
        }
    }
}