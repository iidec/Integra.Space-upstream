//-----------------------------------------------------------------------
// <copyright file="FirstLevelPipelineExecutor.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline
{
    /// <summary>
    /// Command pipeline builder.
    /// </summary>
    internal class FirstLevelPipelineExecutor : PipelineExecutorBase<FirstLevelPipelineContext, FirstLevelPipelineContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FirstLevelPipelineExecutor"/> class.
        /// </summary>
        /// <param name="pipeline">Pipeline to execute.</param>
        public FirstLevelPipelineExecutor(Filter<FirstLevelPipelineContext, FirstLevelPipelineContext> pipeline) : base(pipeline)
        {
        }

        /// <inheritdoc />
        public override FirstLevelPipelineContext Execute(FirstLevelPipelineContext context)
        {
            try
            {
                FirstLevelPipelineContext result = this.Pipeline.Execute(context);
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
