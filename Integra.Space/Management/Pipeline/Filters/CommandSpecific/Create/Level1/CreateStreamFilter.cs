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
    using Common;
    using Database;

    /// <summary>
    /// Filter create source class.
    /// </summary>
    internal class CreateStreamFilter : CreateEntityFilter<Language.CreateStreamNode, StreamOptionEnum>
    {
        /// <inheritdoc />
        protected override void CreateEntity(Language.CreateStreamNode command, Dictionary<StreamOptionEnum, object> options, Login login, DatabaseUser user, Schema schema, SpaceDbContext databaseContext)
        {
            if (!string.IsNullOrWhiteSpace(command.Query))
            {
                Space.Database.Stream stream = new Space.Database.Stream();
                stream.ServerId = schema.ServerId;
                stream.DatabaseId = schema.DatabaseId;
                stream.SchemaId = schema.SchemaId;
                stream.StreamId = Guid.NewGuid();
                stream.StreamName = command.MainCommandObject.Name;
                stream.Query = command.Query;

                // se le establece como propietario al usuario que lo esta creando.
                stream.OwnerServerId = user.ServerId;
                stream.OwnerDatabaseId = user.DatabaseId;
                stream.OwnerId = user.DbUsrId;

                // especifico el assembly.
                string assemblyPath = System.IO.Path.Combine(Environment.CurrentDirectory, "TempQueryAssemblies", schema.Database.Server.ServerName, schema.Database.DatabaseName, schema.SchemaName, command.MainCommandObject.Name + Language.Runtime.SpaceAssemblyBuilder.FILEEXTENSION);
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
                if (command.Options.ContainsKey(Common.StreamOptionEnum.Status))
                {
                    stream.IsActive = (bool)command.Options[Common.StreamOptionEnum.Status];
                }

                // almaceno la nueva entidad y guardo los cambios
                databaseContext.Streams.Add(stream);
                databaseContext.SaveChanges();
            }
            else
            {
                throw new Exception("The query of the stream cannot be null neither whitespace.");
            }
        }
    }
}
