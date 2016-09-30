//-----------------------------------------------------------------------
// <copyright file="CreateEntityFilter.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    /// <summary>
    /// Filter create source class.
    /// </summary>
    internal abstract class CreateEntityFilter : CommandFilter
    {
        /// <inheritdoc />
        public override PipelineContext Execute(PipelineContext context)
        {
            this.CreateEntity(context);
            return context;
        }

        /// <inheritdoc />
        public override void OnError(PipelineContext context)
        {
        }

        /// <summary>
        /// Creates a new entity.
        /// </summary>
        /// <param name="context">Context of the pipeline.</param>
        protected abstract void CreateEntity(PipelineContext context);
    }
}
