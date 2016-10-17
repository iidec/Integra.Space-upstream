//-----------------------------------------------------------------------
// <copyright file="DropSchemaFilter.cs" company="Integra.Space">
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
    internal class DropSchemaFilter : DropEntityFilter
    {
        /// <inheritdoc />
        protected override void DropEntity(SpaceDbContext databaseContext, Schema schema, string name)
        {
            Schema schemaToDrop = databaseContext.Schemas.Single(x => x.ServerId == schema.ServerId
                                            && x.DatabaseId == schema.DatabaseId
                                            && x.SchemaName == name);

            databaseContext.Schemas.Remove(schemaToDrop);
            databaseContext.SaveChanges();
        }
    }
}
