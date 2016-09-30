//-----------------------------------------------------------------------
// <copyright file="CreateSourceFilter.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using System.Linq;
    using Database;
    using Ninject;

    /// <summary>
    /// Filter create source class.
    /// </summary>
    internal class CreateSourceFilter : CreateEntityFilter
    {
        /// <inheritdoc />
        protected override void CreateEntity(PipelineContext context)
        {
            Source source = new Source();
            source.ServerId = context.CommandContext.Schema.ServerId;
            source.DatabaseId = context.CommandContext.Schema.DatabaseId;
            source.SchemaId = context.CommandContext.Schema.SchemaId;
            source.SourceId = Guid.NewGuid();
            source.SourceName = ((Language.DDLCommand)context.CommandContext.Command).MainCommandObject.Name;

            // se le establece como propietario al usuario que lo esta creando
            source.OwnerServerId = context.SecurityContext.User.ServerId;
            source.OwnerDatabaseId = context.SecurityContext.User.DatabaseId;
            source.OwnerId = context.SecurityContext.User.DbUsrId;
            
            // almaceno la nueva entidad y guardo los cambios
            SpaceDbContext databaseContext = context.Kernel.Get<SpaceDbContext>();
            databaseContext.Sources.Add(source);
            databaseContext.SaveChanges();
        }
    }
}
