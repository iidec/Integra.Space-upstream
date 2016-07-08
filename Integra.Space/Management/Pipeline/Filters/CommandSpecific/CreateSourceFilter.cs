//-----------------------------------------------------------------------
// <copyright file="CreateSourceFilter.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using Models;
    using Ninject;
    using Repos;

    /// <summary>
    /// Filter create source class.
    /// </summary>
    internal class CreateSourceFilter : CommandFilter
    {
        /// <inheritdoc />
        public override PipelineExecutionCommandContext Execute(PipelineExecutionCommandContext context)
        {
            this.CreateSource(context);
            return context;
        }

        /// <inheritdoc />
        public override void OnError(PipelineExecutionCommandContext context)
        {
            if (!this.Executed)
            {
                return;
            }

            this.DeleteSource(context);
        }

        /// <summary>
        /// Creates a new source.
        /// </summary>
        /// <param name="context">Context of the pipeline.</param>
        private void CreateSource(PipelineExecutionCommandContext context)
        {
            Source source = new Source(Guid.NewGuid(), context.Command.ObjectName);
            CacheRepositoryBase<Source> sr = context.Kernel.Get<SourceCacheRepository>();
            sr.Add(source);
        }

        /// <summary>
        /// Delete a the source.
        /// </summary>
        /// <param name="context">Context of the pipeline.</param>
        private void DeleteSource(PipelineExecutionCommandContext context)
        {
            CacheRepositoryBase<Source> sr = context.Kernel.Get<SourceCacheRepository>();
            Source source = sr.FindByName(context.Command.ObjectName);
            if (source != null)
            {
                sr.Delete(source);
            }
        }
    }
}
