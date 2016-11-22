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
            // obtengo la fuente.
            Source source = databaseContext.Sources.Single(x => x.ServerId == schema.ServerId
                                            && x.DatabaseId == schema.DatabaseId
                                            && x.SchemaId == schema.SchemaId
                                            && x.SourceName == name);

            if (source.Streams.Count > 0)
            {
                throw new System.Exception("Can't delete a source while is being used by one or more streams.");
            }
            else
            {
                // elimino las columnas de la fuente.
                databaseContext.SourceColumns.RemoveRange(source.Columns);
                databaseContext.SaveChanges();

                // elimino la fuente.
                databaseContext.Sources.Remove(source);
                databaseContext.SaveChanges();
            }
        }
    }
}
