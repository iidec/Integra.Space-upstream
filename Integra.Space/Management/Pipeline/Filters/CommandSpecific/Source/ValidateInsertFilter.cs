//-----------------------------------------------------------------------
// <copyright file="ValidateInsertFilter.cs" company="Integra.Space">
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
    /// Truncate command action class.
    /// </summary>
    internal class ValidateInsertFilter : CommandFilter
    {
        /// <inheritdoc />
        public override PipelineContext Execute(PipelineContext context)
        {
            Language.InsertNode command = (Language.InsertNode)context.CommandContext.Command;
            SpaceDbContext databaseContext = context.Kernel.Get<SpaceDbContext>();
            Login login = context.SecurityContext.Login;
            Schema schema = command.MainCommandObject.GetSchema(databaseContext, login);
            Source source = databaseContext.Sources.Single(x => x.ServerId == schema.ServerId
                                            && x.DatabaseId == schema.DatabaseId
                                            && x.SchemaId == schema.SchemaId
                                            && x.SourceName == command.MainCommandObject.Name);

            IEnumerable<IGrouping<string, SourceColumn>> sourceColumns = source.Columns
                                                                            .GroupBy(x => x.ColumnType)
                                                                            .Where(x => x.Key == typeof(string).AssemblyQualifiedName);

            IGrouping<string, SourceColumn> stringGroup = sourceColumns.SingleOrDefault(x => x.Key == typeof(string).AssemblyQualifiedName);
            if (stringGroup != null)
            {
                stringGroup.ToList().ForEach(x =>
                {
                    if (!command.ColumnsWithValues.ContainsKey(x.ColumnName))
                    {
                        throw new Exception(string.Format("The column '{0}' must be initialized in the insert command.", x.ColumnName));
                    }
                    else
                    {
                        if (command.ColumnsWithValues[x.ColumnName] != null)
                        {
                            if (!(command.ColumnsWithValues[x.ColumnName].ToString().Length <= x.ColumnLength))
                            {
                                throw new Exception(string.Format("The column length of '{0}' must be less than or equal {1}.", x.ColumnName, x.ColumnLength));
                            }
                        }
                    }
                });
            }

            return context;
        }
    }
}