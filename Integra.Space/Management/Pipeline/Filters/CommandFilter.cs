//-----------------------------------------------------------------------
// <copyright file="CommandFilter.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using System.Diagnostics.Contracts;
    using Integra.Space.Pipeline;
    using Models;
    using Ninject;
    using Repos;

    /// <summary>
    /// Command filter.
    /// </summary>
    internal abstract class CommandFilter : Filter<PipelineContext, PipelineContext>
    {
        /// <inheritdoc />
        public override PipelineContext Execute(PipelineContext context)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override void OnError(PipelineContext context)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the schema used by the command.
        /// </summary>
        /// <param name="context">Context of the pipeline</param>
        /// <returns>The schema used by the command.</returns>
        protected Models.Schema GetSchema(PipelineContext context)
        {
            if (context.Command.SchemaName == null)
            {
                return context.User.DefaultSchema;
            }
            else
            {
                string schemaName = context.Command.SchemaName;
                SchemaCacheRepository schemaRepo = (SchemaCacheRepository)context.Kernel.Get<IRepository<Schema>>();
                Schema schema = schemaRepo.FindByName(context.Command.SchemaName);

                Contract.EnsuresOnThrow<Exception>(schema != null, "The schema '" + schemaName + "' does not exist.");
                return schema;
            }
        }
    }
}
