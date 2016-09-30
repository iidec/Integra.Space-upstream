//-----------------------------------------------------------------------
// <copyright file="AlterEntityFilter.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    /// <summary>
    /// Filter create source class.
    /// </summary>
    internal abstract class AlterEntityFilter : CommandFilter
    {
        /// <inheritdoc />
        public override PipelineContext Execute(PipelineContext context)
        {
            this.EditEntity(context);
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
        protected abstract void EditEntity(PipelineContext context);
    }
}
