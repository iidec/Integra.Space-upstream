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
        protected override Stream CreateEntity(PipelineContext context)
        {
            if (context.Command is Language.CreateAndAlterStreamNode)
            {
                string query = ((Language.CreateAndAlterStreamNode)context.Command).Query;
                if (!string.IsNullOrWhiteSpace(query))
                {
                    return new Stream(Guid.NewGuid(), context.Command.ObjectName, query, this.GetSchema(context));
                }
                else
                {
                    throw new Exception("The query of the stream cannot be null neither whitespace.");
                }
            }
            else
            {
                throw new Exception("Wrong command for the filter.");
            }
        }
    }
}
