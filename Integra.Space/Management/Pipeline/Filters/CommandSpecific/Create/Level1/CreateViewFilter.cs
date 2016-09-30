//-----------------------------------------------------------------------
// <copyright file="CreateViewFilter.cs" company="Integra.Space">
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
    internal class CreateViewFilter : CreateEntityFilter
    {
        /// <inheritdoc />
        protected override void CreateEntity(PipelineContext context)
        {
            View view = new View();
            view.ServerId = context.CommandContext.Schema.ServerId;
            view.DatabaseId = context.CommandContext.Schema.DatabaseId;
            view.SchemaId = context.CommandContext.Schema.SchemaId;
            view.ViewId = Guid.NewGuid();
            view.ViewName = ((Language.DDLCommand)context.CommandContext.Command).MainCommandObject.Name;
            view.Predicate = string.Empty;

            // se le establece como propietario al usuario que lo esta creando
            view.OwnerServerId = context.SecurityContext.User.ServerId;
            view.OwnerDatabaseId = context.SecurityContext.User.DatabaseId;
            view.OwnerId = context.SecurityContext.User.DbUsrId;

            // almaceno la nueva entidad y guardo los cambios
            SpaceDbContext databaseContext = context.Kernel.Get<SpaceDbContext>();
            databaseContext.Views.Add(view);
            databaseContext.SaveChanges();
        }
    }
}
