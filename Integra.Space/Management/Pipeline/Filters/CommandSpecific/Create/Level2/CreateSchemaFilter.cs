//-----------------------------------------------------------------------
// <copyright file="CreateSchemaFilter.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using System.Collections.Generic;
    using Common;
    using Database;

    /// <summary>
    /// Filter create source class.
    /// </summary>
    internal class CreateSchemaFilter : CreateEntityFilter<Language.CreateSchemaNode, SchemaOptionEnum>
    {
        /// <inheritdoc />
        protected override void CreateEntity(Language.CreateSchemaNode command, Dictionary<SchemaOptionEnum, object> options, Login login, DatabaseUser user, Schema schema, SpaceDbContext databaseContext)
        {
            Schema newSchema = new Schema();
            newSchema.ServerId = schema.ServerId;
            newSchema.DatabaseId = schema.DatabaseId;
            newSchema.SchemaId = Guid.NewGuid();
            newSchema.SchemaName = command.MainCommandObject.Name;

            newSchema.OwnerServerId = user.ServerId;
            newSchema.OwnerDatabaseId = user.DatabaseId;
            newSchema.OwnerId = user.DbUsrId;

            // almaceno la nueva entidad y guardo los cambios
            databaseContext.Schemas.Add(newSchema);
            databaseContext.SaveChanges();
        }
    }
}
