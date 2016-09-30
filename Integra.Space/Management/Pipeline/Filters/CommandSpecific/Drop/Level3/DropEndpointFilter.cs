//-----------------------------------------------------------------------
// <copyright file="DropEndpointFilter.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System.Linq;
    using Database;
    using Integra.Space.Pipeline;
    using Ninject;

    /// <summary>
    /// Drop entity class.
    /// </summary>
    internal class DropEndpointFilter : DropEntityFilter
    {
        /// <inheritdoc />
        protected override void DropEntity(PipelineContext context)
        {
            SpaceDbContext databaseContext = context.Kernel.Get<SpaceDbContext>();
            Endpoint endpoint = databaseContext.Endpoints.Single(x => x.ServerId == context.CommandContext.Schema.ServerId
                                            && x.EnpointName == ((Language.DDLCommand)context.CommandContext.Command).MainCommandObject.Name);

            databaseContext.Endpoints.Remove(endpoint);
            databaseContext.SaveChanges();
        }
    }
}
