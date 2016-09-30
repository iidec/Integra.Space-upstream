//-----------------------------------------------------------------------
// <copyright file="AlterSourceFilter.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database;
    using Ninject;

    /// <summary>
    /// Filter alter user class.
    /// </summary>
    internal class AlterSourceFilter : AlterEntityFilter
    {
        /// <inheritdoc />
        protected override void EditEntity(PipelineContext context)
        {
            Dictionary<Common.SourceOptionEnum, object> options = ((Language.CreateObjectNode<Common.SourceOptionEnum>)context.CommandContext.Command).Options;

            SpaceDbContext databaseContext = context.Kernel.Get<SpaceDbContext>();
            Source source = databaseContext.Sources.Single(x => x.ServerId == context.CommandContext.Schema.ServerId
                                            && x.DatabaseId == context.CommandContext.Schema.DatabaseId
                                            && x.SchemaId == context.CommandContext.Schema.SchemaId
                                            && x.SourceName == ((Language.DDLCommand)context.CommandContext.Command).MainCommandObject.Name);
            
            if (options.ContainsKey(Common.SourceOptionEnum.Name))
            {
                source.SourceName = options[Common.SourceOptionEnum.Name].ToString();
            }

            databaseContext.SaveChanges();
        }
    }
}
