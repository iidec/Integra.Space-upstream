//-----------------------------------------------------------------------
// <copyright file="CommandPipelineExecutor.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline
{
    /// <summary>
    /// Command pipeline builder.
    /// </summary>
    /// <typeparam name="TInput">Input type of the pipeline.</typeparam>
    /// <typeparam name="TOutput">Output type of the pipeline.</typeparam>
    internal class CommandPipelineExecutor<TInput, TOutput>
    {
        /// <summary>
        /// Pipeline context.
        /// </summary>
        private Filter<TInput, TOutput> pipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandPipelineExecutor{TInput, TOutput}"/> class.
        /// </summary>
        /// <param name="pipeline">Pipeline to execute.</param>
        public CommandPipelineExecutor(Filter<TInput, TOutput> pipeline)
        {
            this.pipeline = pipeline;
        }

        /// <summary>
        /// Build the pipeline.
        /// </summary>
        /// <param name="input">Input of the pipeline.</param>
        /// <returns>The result of the pipeline execution.</returns>
        public TOutput Execute(TInput input)
        {
            try
            {
                return this.pipeline.Execute(input);
            }
            catch (System.Exception e)
            {
                this.pipeline.OnError(e);
                throw e;
            }
        }
    }
}
