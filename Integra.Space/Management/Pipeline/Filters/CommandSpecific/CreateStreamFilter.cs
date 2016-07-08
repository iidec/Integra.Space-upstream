//-----------------------------------------------------------------------
// <copyright file="CreateStreamFilter.cs" company="Integra.Space">
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
    internal class CreateStreamFilter : CreateEntityFilter<Stream>
    {
        /// <inheritdoc />
        protected override Stream CreateEntity(PipelineExecutionCommandContext context)
        {
            return new Stream(Guid.NewGuid(), context.Command.ObjectName, ((Language.CreateAndAlterStreamNode)context.Command).Query);
        }
    }
}
