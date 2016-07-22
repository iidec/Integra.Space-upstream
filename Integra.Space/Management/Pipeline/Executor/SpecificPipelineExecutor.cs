//-----------------------------------------------------------------------
// <copyright file="SpecificPipelineExecutor.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline
{
    using System;

    /// <summary>
    /// Command pipeline builder.
    /// </summary>
    internal class SpecificPipelineExecutor : PipelineExecutorBase<PipelineContext, PipelineContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpecificPipelineExecutor"/> class.
        /// </summary>
        /// <param name="pipeline">Pipeline to execute.</param>
        public SpecificPipelineExecutor(Filter<PipelineContext, PipelineContext> pipeline) : base(pipeline)
        {
        }

        /// <inheritdoc />
        public override PipelineContext Execute(PipelineContext context)
        {
            try
            {
                PipelineContext result = this.Pipeline.Execute(context);
                this.Pipeline.Executed = true;
                return result;
            }
            catch (System.Exception e)
            {
                context.Error = e;
                this.Pipeline.OnError(context);
                throw e;
            }
        }
    }
}
