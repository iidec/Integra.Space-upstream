//-----------------------------------------------------------------------
// <copyright file="DropStreamFilter.cs" company="Integra.Space">
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
    internal class DropStreamFilter : DropEntityFilter
    {
        /// <inheritdoc />
        protected override void DropEntity(PipelineContext context)
        {
            SpaceDbContext databaseContext = context.Kernel.Get<SpaceDbContext>();
            Stream stream = databaseContext.Streams.Single(x => x.ServerId == context.CommandContext.Schema.ServerId
                                            && x.DatabaseId == context.CommandContext.Schema.DatabaseId
                                            && x.SchemaId == context.CommandContext.Schema.SchemaId
                                            && x.StreamName == ((Language.DDLCommand)context.CommandContext.Command).MainCommandObject.Name);

            databaseContext.Streams.Remove(stream);
            databaseContext.SaveChanges();
        }
    }
}
