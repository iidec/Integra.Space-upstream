//-----------------------------------------------------------------------
// <copyright file="PipelineBuilderBase.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Management.Pipeline
{
    /// <summary>
    /// Pipeline builder base class.
    /// </summary>
    /// <typeparam name="TContext">Context of the pipeline to build.</typeparam>
    internal abstract class PipelineBuilderBase<TContext>
    {
        /// <summary>
        /// Pipeline context.
        /// </summary>
        private TContext context;

        /// <summary>
        /// Initializes a new instance of the <see cref="PipelineBuilderBase{TContext}"/> class.
        /// </summary>
        /// <param name="context">Command pipeline context.</param>
        public PipelineBuilderBase(TContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Build the pipeline.
        /// </summary>
        /// <typeparam name="TIn">Input type.</typeparam>
        /// <typeparam name="TOut">Output type.</typeparam>
        public abstract void Build<TIn, TOut>();
    }
}
