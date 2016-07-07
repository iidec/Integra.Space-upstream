//-----------------------------------------------------------------------
// <copyright file="CommandActionFilterGenerator.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using Common.CommandContext;

    /// <summary>
    /// Create command action class.
    /// </summary>
    internal abstract class CommandActionFilterGenerator : Filter<PipelineCommandContext, PipelineCommandContext>
    {
        /// <inheritdoc />
        public override PipelineCommandContext Execute(PipelineCommandContext input)
        {
            Console.WriteLine("Se ejecutó la acción específica: " + input.Command.Action);
            return input;
        }
    }
}