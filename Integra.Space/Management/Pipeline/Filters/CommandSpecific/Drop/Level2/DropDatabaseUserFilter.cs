//-----------------------------------------------------------------------
// <copyright file="DropDatabaseUserFilter.cs" company="Integra.Space">
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
    internal class DropDatabaseUserFilter : DropEntityFilter
    {
        /// <inheritdoc />
        protected override void DropEntity(PipelineContext context)
        {
            SpaceDbContext databaseContext = context.Kernel.Get<SpaceDbContext>();
            DatabaseUser databaseUser = databaseContext.DatabaseUsers.Single(x => x.ServerId == context.CommandContext.Schema.ServerId
                                            && x.DatabaseId == context.CommandContext.Schema.DatabaseId
                                            && x.DbUsrName == ((Language.DDLCommand)context.CommandContext.Command).MainCommandObject.Name);

            databaseContext.DatabaseUsers.Remove(databaseUser);
            databaseContext.SaveChanges();
        }
    }
}
