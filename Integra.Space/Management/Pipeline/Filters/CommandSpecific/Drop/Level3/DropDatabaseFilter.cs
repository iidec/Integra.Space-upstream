//-----------------------------------------------------------------------
// <copyright file="DropDatabaseFilter.cs" company="Integra.Space">
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
    internal class DropDatabaseFilter : DropEntityFilter
    {
        /// <inheritdoc />
        protected override void DropEntity(PipelineContext context)
        {
            SpaceDbContext databaseContext = context.Kernel.Get<SpaceDbContext>();
            Database database = databaseContext.Databases.Single(x => x.ServerId == context.CommandContext.Schema.ServerId
                                            && x.DatabaseName == ((Language.DDLCommand)context.CommandContext.Command).MainCommandObject.Name);

            databaseContext.Databases.Remove(database);
            databaseContext.SaveChanges();
        }
    }
}
