//-----------------------------------------------------------------------
// <copyright file="DropEntityFilter.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using Integra.Space.Pipeline;

    /// <summary>
    /// Drop entity class.
    /// </summary>
    internal abstract class DropEntityFilter : CommandFilter
    {
        /// <inheritdoc />
        public override PipelineContext Execute(PipelineContext context)
        {
            this.DropEntity(context);
            return context;
        }

        /// <inheritdoc />
        public override void OnError(PipelineContext context)
        {
        }

        /// <summary>
        /// Removes the specified entity.
        /// </summary>
        /// <param name="context">Pipeline context.</param>
        protected virtual void DropEntity(PipelineContext context)
        {
        }
    }
}
