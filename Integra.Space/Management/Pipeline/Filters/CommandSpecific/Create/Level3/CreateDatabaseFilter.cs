//-----------------------------------------------------------------------
// <copyright file="CreateDatabaseFilter.cs" company="Integra.Space">
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
    internal class CreateDatabaseFilter : CreateEntityFilter
    {
        /// <inheritdoc />
        protected override void CreateEntity(PipelineContext context)
        {
            Database database = new Database();
            database.ServerId = context.CommandContext.Schema.ServerId;
            database.DatabaseId = Guid.NewGuid();
            database.DatabaseName = ((Language.DDLCommand)context.CommandContext.Command).MainCommandObject.Name;

            // se le establece como propietario al login que esta creando la entidad
            database.OwnerServerId = context.SecurityContext.Login.ServerId;
            database.OwnerId = context.SecurityContext.Login.LoginId;
            
            // almaceno la nueva entidad y guardo los cambios
            SpaceDbContext databaseContext = context.Kernel.Get<SpaceDbContext>();
            databaseContext.Databases.Add(database);
            databaseContext.SaveChanges();
        }
    }
}
