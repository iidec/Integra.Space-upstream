//-----------------------------------------------------------------------
// <copyright file="DropSourceFilter.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System.Linq;
    using Database;

    /// <summary>
    /// Drop entity class.
    /// </summary>
    internal class DropSourceFilter : DropEntityFilter
    {
        /// <inheritdoc />
        protected override void DropEntity(SpaceDbContext databaseContext, Schema schema, string name)
        {
            Source source = databaseContext.Sources.Single(x => x.ServerId == schema.ServerId
                                            && x.DatabaseId == schema.DatabaseId
                                            && x.SchemaId == schema.SchemaId
                                            && x.SourceName == name);

            databaseContext.Sources.Remove(source);
            databaseContext.SaveChanges();
        }
    }
}
