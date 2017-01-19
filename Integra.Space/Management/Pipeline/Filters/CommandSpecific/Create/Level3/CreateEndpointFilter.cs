//-----------------------------------------------------------------------
// <copyright file="CreateEndpointFilter.cs" company="Integra.Space">
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
    internal class CreateEndpointFilter : CreateEntityFilter
    {
        /// <inheritdoc />
        protected override void CreateEntity(PipelineContext context)
        {
            Endpoint endpoint = new Endpoint();
            endpoint.ServerId = context.CommandContext.Schema.ServerId;
            endpoint.EndpointId = Guid.NewGuid();
            endpoint.EnpointName = ((Language.DDLCommand)context.CommandContext.Command).MainCommandObject.Name;

            // se le establece como propietario al login que esta creando la entidad
            endpoint.OwnerServerId = context.SecurityContext.Login.ServerId;
            endpoint.OwnerId = context.SecurityContext.Login.LoginId;
            
            // almaceno la nueva entidad y guardo los cambios
            SpaceDbContext databaseContext = context.Kernel.Get<SpaceDbContext>();
            databaseContext.Endpoints.Add(endpoint);
            databaseContext.SaveChanges();
        }
    }
}
