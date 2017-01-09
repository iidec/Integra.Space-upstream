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
        protected override void DropEntity(SpaceDbContext databaseContext, Schema schema, string name)
        {
            Endpoint endpoint = databaseContext.Endpoints.Single(x => x.ServerId == schema.ServerId
                                            && x.EnpointName == name);

            databaseContext.Endpoints.Remove(endpoint);
            databaseContext.SaveChanges();
        }
    }
}
