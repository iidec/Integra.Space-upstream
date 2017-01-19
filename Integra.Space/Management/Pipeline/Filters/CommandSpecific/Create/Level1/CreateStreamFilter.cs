//-----------------------------------------------------------------------
// <copyright file="CreateStreamFilter.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Common;
    using Compiler;
    using Database;
    using Language;

    /// <summary>
    /// Filter create source class.
    /// </summary>
    internal class CreateStreamFilter : CreateEntityFilter<CreateStreamNode, StreamOptionEnum>
    {
        /// <inheritdoc />
        protected override void CreateEntity(CreateStreamNode command, Dictionary<StreamOptionEnum, object> options, Login login, DatabaseUser user, Schema schema, SpaceDbContext databaseContext)
        {
            if (!string.IsNullOrWhiteSpace(command.Query))
            {
                Space.Database.Stream stream = new Space.Database.Stream();
                stream.ServerId = schema.ServerId;
                stream.DatabaseId = schema.DatabaseId;
                stream.SchemaId = schema.SchemaId;
                stream.StreamId = Guid.NewGuid();
                stream.StreamName = command.MainCommandObject.Name;
                stream.Query = command.Query.Trim();

                // se le establece como propietario al usuario que lo esta creando.
                stream.OwnerServerId = user.ServerId;
                stream.OwnerDatabaseId = user.DatabaseId;
                stream.OwnerId = user.DbUsrId;

                // especifico el assembly.
                string assemblyName = string.Format("{0}_{1}_{2}_{3}.{4}", schema.Database.Server.ServerName, schema.Database.DatabaseName, schema.SchemaName, command.MainCommandObject.Name, SpaceAssemblyBuilder.FILEEXTENSION);
                string assemblyPath = Path.Combine(Environment.CurrentDirectory, "TempQueryAssemblies", schema.Database.Server.ServerName, schema.Database.DatabaseName, schema.SchemaName, assemblyName);
                if (File.Exists(assemblyPath))
                {
                    try
                    {
                        stream.Assembly = File.ReadAllBytes(assemblyPath);
                    }
                    catch (Exception e)
                    {
                        throw new Exception(string.Format("Can't read the bytes of the assembly of the stream '{0}' at the schema '{1}', database '{2}' and server '{3}'", stream.StreamName, stream.Schema.SchemaName, stream.Schema.Database.DatabaseName, stream.Schema.Database.Server.ServerName), e);
                    }
                }
                else
                {
                    throw new FileNotFoundException(string.Format("The assembly of the stream '{0}' at the schema '{1}', database '{2}' and server '{3}' was not found.", stream.StreamName, stream.Schema.SchemaName, stream.Schema.Database.DatabaseName, stream.Schema.Database.Server.ServerName));
                }

                stream.IsActive = true;
                if (command.Options.ContainsKey(StreamOptionEnum.Status))
                {
                    stream.IsActive = (bool)command.Options[StreamOptionEnum.Status];
                }

                // almaceno la nueva entidad y guardo los cambios
                databaseContext.Streams.Add(stream);
                databaseContext.SaveChanges();

                // guardo la relación del stream con las fuentes a las que hace referencia, de entrada o salida
                CommandObject[] sources = command.CommandObjects.Where(x => x.SecurableClass == SystemObjectEnum.Source).ToArray();
                foreach (CommandObject source in sources)
                {
                    Schema sourceSchema = source.GetSchema(databaseContext, login);
                    Source sourceFromDatabase = databaseContext.Sources.Single(x => x.ServerId == sourceSchema.ServerId && x.DatabaseId == sourceSchema.DatabaseId && x.SchemaId == sourceSchema.SchemaId && x.SourceName == source.Name);

                    bool? isInput = null;
                    if (source.GranularPermission == PermissionsEnum.Read)
                    {
                        isInput = true;
                    }
                    else if (source.GranularPermission == PermissionsEnum.Write)
                    {
                        isInput = false;
                    }

                    SourceByStream relationship = new SourceByStream()
                    {
                        RelationshipId = Guid.NewGuid(),
                        Source = sourceFromDatabase,
                        Stream = stream,
                        IsInputSource = isInput.Value
                    };

                    databaseContext.SourcesByStreams.Add(relationship);
                }

                databaseContext.SaveChanges();

                // agrego las columnas del stream a la base de datos
                List<ProjectionColumn> projectionColumns = command.ExecutionPlan.GetQueryProyection();
                foreach (ProjectionColumn column in projectionColumns)
                {
                    Type columnType = column.ColumnType;

                    // si la columna tiene fuente y el tipo es nulo, se busca de la fuente asociada
                    if (!string.IsNullOrWhiteSpace(column.SourceName) && column.ColumnType == null)
                    {
                        ReferencedSource sourceAux = command.InputSources.Single(x => x.Alias == column.SourceAlias);
                        string databaseName = sourceAux.DatabaseName == null ? schema.Database.DatabaseName : sourceAux.DatabaseName;
                        string schemaName = sourceAux.SchemaName == null ? schema.SchemaName : sourceAux.SchemaName;
                        columnType = databaseContext.Sources
                            .Single(x => x.ServerId == schema.ServerId && x.Schema.Database.DatabaseName == databaseName && x.Schema.SchemaName == schemaName && x.SourceName == sourceAux.SourceName)
                            .Columns
                            .Where(x => x.ColumnName == column.PropertyName)
                            .Select(x => Type.GetType(x.ColumnType))
                            .Single();
                    }

                    StreamColumn projectionColumn = new StreamColumn()
                    {
                        ColumnId = Guid.NewGuid(),
                        Stream = stream,
                        ColumnName = column.Alias,
                        ColumnType = columnType.AssemblyQualifiedName
                    };

                    databaseContext.StreamColumns.Add(projectionColumn);
                }

                databaseContext.SaveChanges();
            }
            else
            {
                throw new Exception("The query of the stream cannot be null neither whitespace.");
            }
        }
    }
}
