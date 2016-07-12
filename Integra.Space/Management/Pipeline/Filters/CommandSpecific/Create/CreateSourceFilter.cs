//-----------------------------------------------------------------------
// <copyright file="CreateSourceFilter.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using Cache;
    using Models;
    using Ninject;
    using Repos;

    /// <summary>
    /// Filter create source class.
    /// </summary>
    internal class CreateSourceFilter : CreateEntityFilter<Source>
    {
        /// <inheritdoc />
        protected override Source CreateEntity(PipelineExecutionCommandContext context)
        {
            return new Source(Guid.NewGuid(), context.Command.ObjectName);
        }
    }
}
