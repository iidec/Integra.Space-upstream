//-----------------------------------------------------------------------
// <copyright file="AlterSchemaFilter.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Common;
    using Database;
    using Language;
    using Ninject;

    /// <summary>
    /// Filter alter user class.
    /// </summary>
    internal class AlterSchemaFilter : AlterEntityFilter
    {
        /// <inheritdoc />
        protected override void EditEntity(PipelineContext context)
        {
            Dictionary<SchemaOptionEnum, object> options = ((Language.CreateObjectNode<SchemaOptionEnum>)context.CommandContext.Command).Options;

            SpaceDbContext databaseContext = context.Kernel.Get<SpaceDbContext>();
            Schema schema = databaseContext.Schemas.Single(x => x.ServerId == context.CommandContext.Schema.ServerId
                                            && x.DatabaseId == context.CommandContext.Schema.DatabaseId
                                            && x.SchemaName == ((Language.DDLCommand)context.CommandContext.Command).MainCommandObject.Name);
            
            if (options.ContainsKey(Common.SchemaOptionEnum.Name))
            {
                schema.SchemaName = options[Common.SchemaOptionEnum.Name].ToString();
            }

            databaseContext.SaveChanges();
        }
    }
}
