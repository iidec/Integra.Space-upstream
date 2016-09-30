//-----------------------------------------------------------------------
// <copyright file="DropDatabaseRoleFilter.cs" company="Integra.Space">
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
    internal class DropDatabaseRoleFilter : DropEntityFilter
    {
        /// <inheritdoc />
        protected override void DropEntity(PipelineContext context)
        {
            SpaceDbContext databaseContext = context.Kernel.Get<SpaceDbContext>();
            DatabaseRole databaseRole = databaseContext.DatabaseRoles.Single(x => x.ServerId == context.CommandContext.Schema.ServerId
                                            && x.DatabaseId == context.CommandContext.Schema.DatabaseId
                                            && x.DbRoleName == ((Language.DDLCommand)context.CommandContext.Command).MainCommandObject.Name);

            databaseContext.DatabaseRoles.Remove(databaseRole);
            databaseContext.SaveChanges();
        }
    }
}
