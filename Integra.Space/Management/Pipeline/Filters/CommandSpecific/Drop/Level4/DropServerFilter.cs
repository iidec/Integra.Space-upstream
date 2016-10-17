//-----------------------------------------------------------------------
// <copyright file="DropServerFilter.cs" company="Integra.Space">
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
    internal class DropServerFilter : DropEntityFilter
    {
        /// <inheritdoc />
        protected override void DropEntity(SpaceDbContext databaseContext, Schema schema, string name)
        {
            Server server = databaseContext.Servers.Single(x => x.ServerName == name);

            databaseContext.Servers.Remove(server);
            databaseContext.SaveChanges();
        }
    }
}
