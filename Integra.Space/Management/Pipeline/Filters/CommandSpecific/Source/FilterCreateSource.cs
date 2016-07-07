//-----------------------------------------------------------------------
// <copyright file="FilterCreateSource.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using System.Linq;
    using Cache;
    using Common.CommandContext;
    using Models;

    /// <summary>
    /// Filter create source class.
    /// </summary>
    internal class FilterCreateSource : Filter<PipelineCommandContext, PipelineCommandContext>
    {
        /// <inheritdoc />
        public override PipelineCommandContext Execute(PipelineCommandContext input)
        {
            Source source = new Source(Guid.NewGuid(), input.Command.ObjectName);
            ICacheRepository<Source> sr = (SourceRepository)input.Kernel.GetService(typeof(SourceRepository));
            sr.Add(source);

            return input;
        }
    }
}
