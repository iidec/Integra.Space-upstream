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
            
            // se le establece como propietario al usuario que lo esta creando
            source.OwnerServerId = user.ServerId;
            source.OwnerDatabaseId = user.DatabaseId;
            source.OwnerId = user.DbUsrId;

            source.IsActive = true;
            if (command.Options.ContainsKey(Common.SourceOptionEnum.Status))
            {
                source.IsActive = (bool)command.Options[Common.SourceOptionEnum.Status];
            }

            // almaceno la nueva entidad y guardo los cambios
            databaseContext.Sources.Add(source);
            databaseContext.SaveChanges();
        }
    }
}
