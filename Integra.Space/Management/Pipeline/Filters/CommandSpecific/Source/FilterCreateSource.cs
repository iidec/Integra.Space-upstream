//-----------------------------------------------------------------------
// <copyright file="FilterCreateSource.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using Common.CommandContext;
    using Models;
    using Ninject;
    using Repos;

    /// <summary>
    /// Filter create source class.
    /// </summary>
    internal class FilterCreateSource : Filter<PipelineCommandContext, PipelineCommandContext>
    {
        /// <inheritdoc />
        public override PipelineCommandContext Execute(PipelineCommandContext context)
        {
            Source source = new Source(Guid.NewGuid(), context.Command.ObjectName);
            CacheRepositoryBase<Source> sr = context.Kernel.Get<SourceCacheRepository>();
            sr.Add(source);

            return context;
        }
        
        /// <inheritdoc />
        public override void OnError(Exception e)
        {
            throw new NotImplementedException();
        }
    }
}
