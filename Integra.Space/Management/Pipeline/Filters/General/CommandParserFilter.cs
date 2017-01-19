//-----------------------------------------------------------------------
// <copyright file="CommandParserFilter.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using System.Linq;
    using Common;
    using Integra.Space.Pipeline;
    using Language;
    using Ninject;

    /// <summary>
    /// Filter command parser class.
    /// </summary>
    internal class CommandParserFilter : FirstLevelCommandFilter
    {
        /// <inheritdoc />
        public override FirstLevelPipelineContext Execute(FirstLevelPipelineContext context)
        {
            CommandParser cp = new CommandParser(context.BatchString, context.Kernel.Get<IGrammarRuleValidator>());
            foreach (SystemCommand command in cp.Evaluate())
            {
                context.Commands.Add(new CommandPipelineNode(command));
            }

            return context;
        }

        /// <inheritdoc />
        public override void OnError(FirstLevelPipelineContext e)
        {
            throw new NotImplementedException();
        }
    }
}
