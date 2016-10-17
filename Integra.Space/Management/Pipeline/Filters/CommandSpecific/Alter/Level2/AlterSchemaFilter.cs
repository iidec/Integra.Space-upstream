//-----------------------------------------------------------------------
// <copyright file="AlterSchemaFilter.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System.Collections.Generic;
    using System.Linq;
    using Common;
    using Database;
    using Language;

    /// <summary>
    /// Filter alter user class.
    /// </summary>
    internal class AlterSchemaFilter : AlterEntityFilter<AlterSchemaNode, SchemaOptionEnum>
    {
        /// <inheritdoc />
        protected override void EditEntity(AlterSchemaNode command, Dictionary<SchemaOptionEnum, object> options, Schema schema, SpaceDbContext databaseContext)
        {
            Schema schemaToEdit = databaseContext.Schemas.Single(x => x.ServerId == schema.ServerId
                                            && x.DatabaseId == schema.DatabaseId
                                            && x.SchemaName == command.MainCommandObject.Name);
            
            if (options.ContainsKey(Common.SchemaOptionEnum.Name))
            {
                schemaToEdit.SchemaName = options[Common.SchemaOptionEnum.Name].ToString();
            }

            databaseContext.SaveChanges();
        }
    }
}
