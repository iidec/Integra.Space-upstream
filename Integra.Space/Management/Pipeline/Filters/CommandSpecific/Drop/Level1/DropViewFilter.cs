//-----------------------------------------------------------------------
// <copyright file="DropViewFilter.cs" company="Integra.Space">
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
    internal class DropViewFilter : DropEntityFilter
    {
        /// <inheritdoc />
        protected override void DropEntity(PipelineContext context)
        {
            SpaceDbContext databaseContext = context.Kernel.Get<SpaceDbContext>();
            View view = databaseContext.Views.Single(x => x.ServerId == context.CommandContext.Schema.ServerId
                                            && x.DatabaseId == context.CommandContext.Schema.DatabaseId
                                            && x.SchemaId == context.CommandContext.Schema.SchemaId
                                            && x.ViewName == ((Language.DDLCommand)context.CommandContext.Command).MainCommandObject.Name);

            databaseContext.Views.Remove(view);
            databaseContext.SaveChanges();
        }
    }
}
