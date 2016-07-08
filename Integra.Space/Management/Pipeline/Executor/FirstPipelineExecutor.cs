//-----------------------------------------------------------------------
// <copyright file="FirstPipelineExecutor.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline
{
    /// <summary>
    /// Command pipeline builder.
    /// </summary>
    internal class FirstPipelineExecutor : PipelineExecutor<PipelineContext, PipelineContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FirstPipelineExecutor"/> class.
        /// </summary>
        /// <param name="pipeline">Pipeline to execute.</param>
        public FirstPipelineExecutor(Filter<PipelineContext, PipelineContext> pipeline) : base(pipeline)
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
