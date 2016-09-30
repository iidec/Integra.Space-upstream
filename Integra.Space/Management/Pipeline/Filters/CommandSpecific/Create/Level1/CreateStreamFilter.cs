//-----------------------------------------------------------------------
// <copyright file="CreateStreamFilter.cs" company="Integra.Space">
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
    internal class CreateStreamFilter : CreateEntityFilter
    {
        /// <inheritdoc />
        protected override void CreateEntity(PipelineContext context)
        {
            Language.CreateStreamNode command = (Language.CreateStreamNode)context.CommandContext.Command;
            Dictionary<StreamOptionEnum, object> options = command.Options;
            Schema schema = context.CommandContext.Schema;
            string query = command.Query;
            if (!string.IsNullOrWhiteSpace(query))
            {
                Stream stream = new Stream();
                stream.ServerId = schema.ServerId;
                stream.DatabaseId = schema.DatabaseId;
                stream.SchemaId = schema.SchemaId;
                stream.StreamId = Guid.NewGuid();
                stream.StreamName = command.MainCommandObject.Name;
                stream.Query = query;

                // se le establece como propietario al usuario que lo esta creando.
                stream.OwnerServerId = context.SecurityContext.User.ServerId;
                stream.OwnerDatabaseId = context.SecurityContext.User.DatabaseId;
                stream.OwnerId = context.SecurityContext.User.DbUsrId;

                // especifíco el assembly.
                stream.Assembly = System.IO.File.ReadAllBytes(context.Assembly.Location);

                // almaceno la nueva entidad y guardo los cambios
                SpaceDbContext databaseContext = context.Kernel.Get<SpaceDbContext>();
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
