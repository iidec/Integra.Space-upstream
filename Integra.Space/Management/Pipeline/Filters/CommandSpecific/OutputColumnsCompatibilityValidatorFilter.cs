//-----------------------------------------------------------------------
// <copyright file="OutputColumnsCompatibilityValidatorFilter.cs" company="Integra.Space">
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
    /// Validate stream columns compatibility filter class.
    /// </summary>
    internal abstract class OutputColumnsCompatibilityValidatorFilter : CommandFilter
    {
        /// <inheritdoc />
        public override PipelineContext Execute(PipelineContext context)
        {
            QueryCommandForMetadataNode command = (QueryCommandForMetadataNode)context.CommandContext.Command;
            SpaceDbContext databaseContext = context.Kernel.Get<SpaceDbContext>();
            Login login = context.SecurityContext.Login;

            this.ValidateSources(command.OutputSource, command.ExecutionPlan, command.InputSources, databaseContext, login);

            return context;
        }

        /// <inheritdoc />
        public override void OnError(PipelineContext context)
        {
            base.OnError(context);
        }

        /// <summary>
        /// Get the input sources for the.
        /// </summary>
        /// <param name="inputSources">Input sources.</param>
        /// <param name="databaseContext">Database context.</param>
        /// <param name="login">Client login.</param>
        /// <returns>A list with Integra.Space.Database.Source objects.</returns>
        protected abstract List<Source> GetInputSources(ReferencedSource[] inputSources, SpaceDbContext databaseContext, Login login);

        /// <summary>
        /// Get the source from the database.
        /// </summary>
        /// <param name="source">Command object source.</param>
        /// <param name="databaseContext">Database context.</param>
        /// <param name="login">Client login.</param>
        /// <returns>The database source.</returns>
        protected Source GetSourceFromDatabase(CommandObject source, SpaceDbContext databaseContext, Login login)
        {
            // obtengo el esquema de la fuente especificada en la consulta.
            Schema schema = source.GetSchema(databaseContext, login);

            // obtengo la fuente almacenada en la base de datos.
            Source sourceFromDatabase = databaseContext.Sources.SingleOrDefault(x => x.ServerId == schema.ServerId
                                            && x.DatabaseId == schema.DatabaseId
                                            && x.SchemaId == schema.SchemaId
                                            && x.SourceName == source.Name);

            return sourceFromDatabase;
        }

        /// <summary>
        /// Validate the referenced sources in the query.
        /// </summary>
        /// <param name="outputSource">Output source.</param>
        /// <param name="executionPlan">Execution plan.</param>
        /// <param name="inputSources">Input sources.</param>
        /// <param name="databaseContext">Database context.</param>
        /// <param name="login">Login of the client.</param>
        protected void ValidateSources(CommandObject outputSource, PlanNode executionPlan, ReferencedSource[] inputSources, SpaceDbContext databaseContext, Login login)
        {
            // obtengo la fuente almacenada en la base de datos.
            Source outputSourceFromDatabase = this.GetSourceFromDatabase(outputSource, databaseContext, login);

            if (outputSourceFromDatabase == null)
            {
                throw new Exception(string.Format("Invalid output source {0}.", outputSource.Name));
            }

            List<Source> databaseInputSources = this.GetInputSources(inputSources, databaseContext, login);

            // obtengo las columnas de la fuente
            SourceColumnAux[] outputSourceColumns = outputSourceFromDatabase.Columns.Select(x => new SourceColumnAux(x.ColumnName, Type.GetType(x.ColumnType))).ToArray();

            // obtengo las columnas de la proyección del stream
            List<ProjectionColumn> projectionColumns = executionPlan.GetQueryProyection();

            // verifico que todas las columnas de la proyeccion existan en la fuente especificada.
            foreach (ProjectionColumn projectionColumn in projectionColumns)
            {
                SourceColumnAux outputSourceColumn = outputSourceColumns.FirstOrDefault(x => x.PropertyName == projectionColumn.Alias);

                if (outputSourceColumn == null)
                {
                    throw new Exception("Invalid projection column. All projection columns must exist as source columns.");
                }

                if (outputSourceColumn.PropertyType == projectionColumn.ColumnType)
                {
                    continue;
                }
                else
                {
                    if (projectionColumn.ColumnType == null || projectionColumn.ColumnType == typeof(object))
                    {
                        Source inputSource = databaseInputSources.FirstOrDefault(x => x.SourceName.Equals(projectionColumn.SourceName, StringComparison.InvariantCultureIgnoreCase));

                        // si inputSource es null quiere decir que es una constante la que se esta proyectando.
                        if (inputSource == null)
                        {
                            throw new Exception("Invalid projection column.");
                        }
                        else
                        { 
                            SourceColumn inputSourceColumn = inputSource.Columns.SingleOrDefault(x => x.ColumnName == projectionColumn.PropertyName);

                            if (inputSourceColumn != null)
                            {
                                Type inputSourceColumnType = Type.GetType(inputSourceColumn.ColumnType);

                                if (inputSourceColumnType != outputSourceColumn.PropertyType)
                                {
                                    throw new Exception(string.Format("Invalid projection column. The types between {0} and output source column {1} differed. {2} not equal to {3}.", projectionColumn.Alias, outputSourceColumn.PropertyName, inputSourceColumnType.Name, outputSourceColumn.PropertyType.Name));
                                }
                            }
                            else
                            {
                                throw new Exception("Invalid projection column, it must exist as column at the input source.");
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Generates the source from a metadata source type.
        /// </summary>
        /// <param name="systemSource">Metadata source type.</param>
        /// <returns>Integra.Space.Database.Source object representing the metadata source.</returns>
        private Source GenerateMetadataSource(SystemSourceEnum systemSource)
        {
            Source s = new Source();
            s.SourceName = systemSource.ToString();

            Type sourceType = null;

            switch (systemSource)
            {
                case SystemSourceEnum.Servers:
                    sourceType = typeof(Space.Database.Server);
                    break;
                case SystemSourceEnum.ServerRoles:
                    sourceType = typeof(Space.Database.ServerRole);
                    break;
                case SystemSourceEnum.Logins:
                    sourceType = typeof(Space.Database.Login);
                    break;
                case SystemSourceEnum.Databases:
                    sourceType = typeof(Space.Database.Database);
                    break;
                case SystemSourceEnum.DatabaseRoles:
                    sourceType = typeof(Space.Database.DatabaseRole);
                    break;
                case SystemSourceEnum.Users:
                    sourceType = typeof(Space.Database.DatabaseUser);
                    break;
                case SystemSourceEnum.Schemas:
                    sourceType = typeof(Space.Database.Schema);
                    break;
                case SystemSourceEnum.Sources:
                    sourceType = typeof(Space.Database.Source);
                    break;
                case SystemSourceEnum.SourceColumns:
                    sourceType = typeof(Space.Database.SourceColumn);
                    break;
                case SystemSourceEnum.Streams:
                    sourceType = typeof(Space.Database.Stream);
                    break;
                case SystemSourceEnum.StreamColumns:
                    sourceType = typeof(Space.Database.StreamColumn);
                    break;
            }

            foreach (var prop in sourceType.GetProperties())
            {
                SourceColumn sc = new SourceColumn();
                sc.ColumnName = prop.Name;
                sc.ColumnType = prop.PropertyType.AssemblyQualifiedName;
                s.Columns.Add(sc);
            }

            return s;
        }

        /// <summary>
        /// Source column class.
        /// </summary>
        private class SourceColumnAux
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SourceColumnAux"/> class.
            /// </summary>
            /// <param name="propertyName">Property name.</param>
            /// <param name="propertyType">Property type.</param>
            public SourceColumnAux(string propertyName, Type propertyType)
            {
                this.PropertyName = propertyName;
                this.PropertyType = propertyType;
            }

            /// <summary>
            /// Gets the property name.
            /// </summary>
            public string PropertyName { get; private set; }

            /// <summary>
            /// Gets the property type.
            /// </summary>
            public Type PropertyType { get; private set; }
        }
    }
}
