//-----------------------------------------------------------------------
// <copyright file="CreateUserFilter.cs" company="Integra.Space">
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
    /// Filter create user class.
    /// </summary>
    internal class CreateUserFilter : CreateEntityFilter
    {
        /// <inheritdoc />
        protected override void CreateEntity(PipelineContext context)
        {
            Dictionary<UserOptionEnum, object> options = ((Language.CreateObjectNode<UserOptionEnum>)context.CommandContext.Command).Options;
            Schema schema = context.CommandContext.Schema;

            DatabaseUser user = new DatabaseUser();
            user.ServerId = context.CommandContext.Schema.ServerId;
            user.DatabaseId = context.CommandContext.Schema.DatabaseId;
            user.DbUsrId = Guid.NewGuid();
            user.DbUsrName = ((Language.DDLCommand)context.CommandContext.Command).MainCommandObject.Name;

            SpaceDbContext databaseContext = context.Kernel.Get<SpaceDbContext>();

            if (options.ContainsKey(UserOptionEnum.Default_Schema))
            {
                string schemaName = options[UserOptionEnum.Default_Schema].ToString();
                Schema defaultSchema = databaseContext.Schemas.Single(x => x.ServerId == schema.ServerId && x.DatabaseId == schema.DatabaseId && x.SchemaName == schemaName);

                user.DefaultSchemaServerId = defaultSchema.ServerId;
                user.DefaultSchemaDatabaseId = defaultSchema.DatabaseId;
                user.DefaultSchemaId = defaultSchema.SchemaId;
            }
            else
            {
                user.DefaultSchemaServerId = schema.ServerId;
                user.DefaultSchemaDatabaseId = schema.DatabaseId;
                user.DefaultSchemaId = schema.SchemaId;
            }

            // almaceno la nueva entidad y guardo los cambios
            databaseContext.DatabaseUsers.Add(user);
            databaseContext.SaveChanges();            
        }
    }
}
