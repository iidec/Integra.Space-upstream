//-----------------------------------------------------------------------
// <copyright file="PipelineExecutorBase.cs" company="Integra.Space">
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
    internal abstract class PipelineExecutorBase<TInput, TOutput> where TInput : class where TOutput : class
    {
        /// <summary>
        /// Pipeline to execute.
        /// </summary>
        private Filter<TInput, TOutput> pipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="PipelineExecutorBase{TInput, TOutput}"/> class.
        /// </summary>
        /// <param name="pipeline">Pipeline to execute.</param>
        public PipelineExecutorBase(Filter<TInput, TOutput> pipeline)
        {
            this.pipeline = pipeline;
        }

        /// <summary>
        /// Gets the pipeline.
        /// </summary>
        protected Filter<TInput, TOutput> Pipeline
        {
            get
            {
                return this.pipeline;
            }
        }

        /// <summary>
        /// Build the pipeline.
        /// </summary>
        /// <param name="input">Input of the pipeline.</param>
        /// <returns>The result of the pipeline execution.</returns>
        public abstract TOutput Execute(TInput input);
    }
}
