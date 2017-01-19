//-----------------------------------------------------------------------
// <copyright file="DropStreamFilter.cs" company="Integra.Space">
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
    internal class DropStreamFilter : DropEntityFilter
    {
        /// <inheritdoc />
        protected override void DropEntity(SpaceDbContext databaseContext, Schema schema, string name)
        {
            Stream stream = databaseContext.Streams.Single(x => x.ServerId == schema.ServerId
                                            && x.DatabaseId == schema.DatabaseId
                                            && x.SchemaId == schema.SchemaId
                                            && x.StreamName == name);

            // elimino las relaciones con las fuentes referenciadas
            databaseContext.SourcesByStreams.RemoveRange(stream.Sources);
            databaseContext.SaveChanges();
            
            // elimino las columnas viejas del stream
            StreamColumn[] oldProyectionColumns = databaseContext.StreamColumns.Where(x => x.ServerId == stream.ServerId
                                                        && x.DatabaseId == stream.DatabaseId
                                                        && x.SchemaId == stream.SchemaId
                                                        && x.StreamId == stream.StreamId).ToArray();

            databaseContext.StreamColumns.RemoveRange(oldProyectionColumns);

            databaseContext.SaveChanges();

            // elimino el stream
            databaseContext.Streams.Remove(stream);
            databaseContext.SaveChanges();
        }
    }
}
