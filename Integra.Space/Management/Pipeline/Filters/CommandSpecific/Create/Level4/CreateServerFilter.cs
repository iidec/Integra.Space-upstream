//-----------------------------------------------------------------------
// <copyright file="CreateServerFilter.cs" company="Integra.Space">
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
    internal class CreateServerFilter : CreateEntityFilter
    {
        /// <inheritdoc />
        protected override void CreateEntity(PipelineContext context)
        {
            Server server = new Server();
            server.ServerId = Guid.NewGuid();
            server.ServerName = ((Language.DDLCommand)context.CommandContext.Command).MainCommandObject.Name;
            
            // almaceno la nueva entidad y guardo los cambios
            SpaceDbContext databaseContext = context.Kernel.Get<SpaceDbContext>();
            databaseContext.Servers.Add(server);
            databaseContext.SaveChanges();
        }
    }
}
