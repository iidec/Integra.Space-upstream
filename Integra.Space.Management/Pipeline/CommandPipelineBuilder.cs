//-----------------------------------------------------------------------
// <copyright file="CommandPipelineBuilder.cs" company="Integra.Space.Language">
//     Copyright (c) Integra.Space.Language. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Language.Pipeline
{
    using CommandContext;

    /// <summary>
    /// Command pipeline builder.
    /// </summary>
    internal class CommandPipelineBuilder
    {
        /// <summary>
        /// Pipeline context.
        /// </summary>
        private PipelineCommandContext context;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandPipelineBuilder"/> class.
        /// </summary>
        /// <param name="context">Command pipeline context.</param>
        public CommandPipelineBuilder(PipelineCommandContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Build the pipeline.
        /// </summary>
        /// <typeparam name="TIn">Input type.</typeparam>
        /// <typeparam name="TOut">Output type.</typeparam>
        public void Build<TIn, TOut>()
        {
            var result =
                new Filters.FilterLock()
                .AddStep(new Filters.VerifyExistence())
                .AddStep(new Filters.ValidateExistence())
                .AddStep(new Filters.ValidateAction())
                .AddStep(new Filters.FilterUnlock())
                .Execute(this.context);
        }
    }
}
