//-----------------------------------------------------------------------
// <copyright file="CreateEntityFilter.cs" company="Integra.Space">
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
    internal class CreateEntityFilter : CommandFilter
    {
        /// <inheritdoc />
        public override PipelineExecutionCommandContext Execute(PipelineExecutionCommandContext context)
        {
            switch (context.Command.SpaceObjectType)
            {
                case Common.SpaceObjectEnum.Source:
                    this.CreateSource(context);
                    break;
                case Common.SpaceObjectEnum.Stream:
                    this.CreateStream(context);
                    break;
                default:
                    throw new Exception(string.Format("Invalid object: {0}", context.Command.SpaceObjectType));
            }

            return context;
        }

        /// <inheritdoc />
        public override void OnError(PipelineExecutionCommandContext context)
        {
            base.OnError(context);

            switch (context.Command.SpaceObjectType)
            {
                case Common.SpaceObjectEnum.Source:
                    this.DeleteSource(context);
                    break;
                case Common.SpaceObjectEnum.Stream:
                    break;
            }
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

        /// <summary>
        /// Creates a new stream.
        /// </summary>
        /// <param name="context">Context of the pipeline.</param>
        private void CreateStream(PipelineExecutionCommandContext context)
        {
        }
    }
}
