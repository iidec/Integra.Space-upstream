//-----------------------------------------------------------------------
// <copyright file="FilterCommandParser.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using Common;
    using Integra.Space.Pipeline;
    using Language;

    /// <summary>
    /// Filter command parser class.
    /// </summary>
    internal class FilterCommandParser : CommandFilter
    {
        /// <inheritdoc />
        public override PipelineContext Execute(PipelineContext context)
        {
            CommandParser cp = new CommandParser(context.CommandString);
            context.Command = cp.Evaluate();
            return context;
        }

        /// <inheritdoc />
        public override void OnError(PipelineContext e)
        {
            throw new NotImplementedException();
        }
    }
}
