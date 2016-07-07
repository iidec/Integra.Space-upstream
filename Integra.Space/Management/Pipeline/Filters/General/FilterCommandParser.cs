//-----------------------------------------------------------------------
// <copyright file="FilterCommandParser.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using Common;
    using Common.CommandContext;
    using Integra.Space.Pipeline;
    using Language;

    /// <summary>
    /// Filter command parser class.
    /// </summary>
    internal class FilterCommandParser : Filter<string, SpaceCommand>
    {
        /// <inheritdoc />
        public override SpaceCommand Execute(string input)
        {
            CommandParser cp = new CommandParser(input);
            SpaceCommand command = cp.Evaluate();
            return command;
        }

        /// <inheritdoc />
        public override void OnError(Exception e)
        {
            throw new NotImplementedException();
        }
    }
}
