//-----------------------------------------------------------------------
// <copyright file="CreateSourceFilter.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using System.Collections.Generic;
    using Database;

    /// <summary>
    /// Filter create source class.
    /// </summary>
    internal class CreateSourceFilter : CreateEntityFilter<Language.CreateSourceNode, Common.SourceOptionEnum>
    {
        /// <inheritdoc />
        protected override void CreateEntity(Language.CreateSourceNode command, Dictionary<Common.SourceOptionEnum, object> options, Login login, DatabaseUser user, Schema schema, SpaceDbContext databaseContext)
        {
            Source source = new Source();
            source.ServerId = schema.ServerId;
            source.DatabaseId = schema.DatabaseId;
            source.SchemaId = schema.SchemaId;
            source.SourceId = Guid.NewGuid();
            source.SourceName = command.MainCommandObject.Name;
            source.CacheDurability = 60;
            source.CacheSize = 100;

            // se le establece como propietario al usuario que lo esta creando
            source.OwnerServerId = user.ServerId;
            source.OwnerDatabaseId = user.DatabaseId;
            source.OwnerId = user.DbUsrId;

            source.IsActive = true;
            if (command.Options.ContainsKey(Common.SourceOptionEnum.Status))
            {
                source.IsActive = (bool)command.Options[Common.SourceOptionEnum.Status];
            }

            if (command.Options.ContainsKey(Common.SourceOptionEnum.Cache_Durability))
            {
                source.CacheDurability = (uint)(int)command.Options[Common.SourceOptionEnum.Cache_Durability];
            }

            if (command.Options.ContainsKey(Common.SourceOptionEnum.Cache_Size))
            {
                source.CacheSize = (uint)(int)command.Options[Common.SourceOptionEnum.Cache_Size];
            }

            if (command.Options.ContainsKey(Common.SourceOptionEnum.Persistent))
            {
                source.Persistent = (bool)command.Options[Common.SourceOptionEnum.Persistent];
            }

            // almaceno la nueva entidad y guardo los cambios
            databaseContext.Sources.Add(source);
            databaseContext.SaveChanges();

            byte index = 1;
            foreach (Language.SourceColumnNode kvp in command.Columns)
            {
                SourceColumn column = new SourceColumn();
                column.ColumnId = Guid.NewGuid();
                column.SourceId = source.SourceId;
                column.SchemaId = source.SchemaId;
                column.DatabaseId = source.DatabaseId;
                column.ServerId = source.ServerId;
                column.ColumnName = kvp.Name;
                column.ColumnType = kvp.Type.ColumnType.AssemblyQualifiedName;
                column.ColumnIndex = index;
                column.ColumnLength = (int?)kvp.Type.Length;
                index++;

                databaseContext.SourceColumns.Add(column);
            }

            // almaceno las columnas de la fuente.
            databaseContext.SaveChanges();
        }
    }
}
