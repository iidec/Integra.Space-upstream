//-----------------------------------------------------------------------
// <copyright file="CreateSchemaFilter.cs" company="Integra.Space">
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
    using Ninject;

    /// <summary>
    /// Filter create source class.
    /// </summary>
    internal class CreateSchemaFilter : CreateEntityFilter
    {
        /// <inheritdoc />
        protected override void CreateEntity(PipelineContext context)
        {
            Dictionary<SchemaOptionEnum, object> options = ((Language.CreateSchemaNode)context.CommandContext.Command).Options;
            Schema executionSchema = context.CommandContext.Schema;

            Schema schema = new Schema();
            schema.ServerId = context.CommandContext.Schema.ServerId;
            schema.DatabaseId = context.CommandContext.Schema.DatabaseId;
            schema.SchemaId = Guid.NewGuid();
            schema.SchemaName = ((Language.DDLCommand)context.CommandContext.Command).MainCommandObject.Name;
                        
            DatabaseUser user = context.SecurityContext.User;
            schema.OwnerServerId = user.ServerId;
            schema.OwnerDatabaseId = user.DatabaseId;
            schema.OwnerId = user.DbUsrId;

            // almaceno la nueva entidad y guardo los cambios
            SpaceDbContext databaseContext = context.Kernel.Get<SpaceDbContext>();
            databaseContext.Schemas.Add(schema);
            databaseContext.SaveChanges();
        }
    }
}
