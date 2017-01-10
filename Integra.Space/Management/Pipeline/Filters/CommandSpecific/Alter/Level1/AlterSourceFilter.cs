//-----------------------------------------------------------------------
// <copyright file="AlterSourceFilter.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System.Collections.Generic;
    using System.Linq;
    using Database;

    /// <summary>
    /// Filter alter user class.
    /// </summary>
    internal class AlterSourceFilter : AlterEntityFilter<Language.AlterSourceNode, Common.SourceOptionEnum>
    {
        /// <inheritdoc />
        protected override void EditEntity(Language.AlterSourceNode command, Dictionary<Common.SourceOptionEnum, object> options, Login login, Schema schema, SpaceDbContext databaseContext)
        {
            Source source = databaseContext.Sources.Single(x => x.ServerId == schema.ServerId
                                            && x.DatabaseId == schema.DatabaseId
                                            && x.SchemaId == schema.SchemaId
                                            && x.SourceName == command.MainCommandObject.Name);

            if (options.ContainsKey(Common.SourceOptionEnum.Name))
            {
                source.SourceName = options[Common.SourceOptionEnum.Name].ToString();
            }

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

            databaseContext.SaveChanges();

            if (command.ColumnsToAdd != null)
            {
                byte index = source.Columns.Max(x => x.ColumnIndex);
                foreach (KeyValuePair<string, System.Type> kvp in command.ColumnsToAdd)
                {
                    SourceColumn column = new SourceColumn();
                    column.ColumnId = System.Guid.NewGuid();
                    column.SourceId = source.SourceId;
                    column.SchemaId = source.SchemaId;
                    column.DatabaseId = source.DatabaseId;
                    column.ServerId = source.ServerId;
                    column.ColumnName = kvp.Key;
                    column.ColumnType = kvp.Value.AssemblyQualifiedName;
                    column.ColumnIndex = ++index;

                    databaseContext.SourceColumns.Add(column);
                }
            }

            databaseContext.SaveChanges();

            if (command.ColumnsToRemove != null)
            {
                IEnumerable<SourceColumn> columnsToRemove = source.Columns.Where(x => command.ColumnsToRemove.ContainsKey(x.ColumnName));
                databaseContext.SourceColumns.RemoveRange(columnsToRemove);
            }

            databaseContext.SaveChanges();

            if (source.Columns.Count == 0)
            {
                throw new System.Exception("Remove all columns of a source is not allowed.");
            }
        }
    }
}
