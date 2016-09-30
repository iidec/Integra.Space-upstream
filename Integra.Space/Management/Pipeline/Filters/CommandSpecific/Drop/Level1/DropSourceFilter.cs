//-----------------------------------------------------------------------
// <copyright file="DropSourceFilter.cs" company="Integra.Space">
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
    internal class DropSourceFilter : DropEntityFilter
    {
        /// <inheritdoc />
        protected override void DropEntity(PipelineContext context)
        {
            SpaceDbContext databaseContext = context.Kernel.Get<SpaceDbContext>();
            Source source = databaseContext.Sources.Single(x => x.ServerId == context.CommandContext.Schema.ServerId
                                            && x.DatabaseId == context.CommandContext.Schema.DatabaseId
                                            && x.SchemaId == context.CommandContext.Schema.SchemaId
                                            && x.SourceName == ((Language.DDLCommand)context.CommandContext.Command).MainCommandObject.Name);

            databaseContext.Sources.Remove(source);
            databaseContext.SaveChanges();
        }
    }
}
