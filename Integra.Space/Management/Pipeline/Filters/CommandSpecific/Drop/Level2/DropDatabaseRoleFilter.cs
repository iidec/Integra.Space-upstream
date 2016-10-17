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
        protected override void DropEntity(SpaceDbContext databaseContext, Schema schema, string name)
        {
            DatabaseRole databaseRole = databaseContext.DatabaseRoles.Single(x => x.ServerId == schema.ServerId
                                            && x.DatabaseId == schema.DatabaseId
                                            && x.DbRoleName == name);

            databaseContext.DatabaseRoles.Remove(databaseRole);
            databaseContext.SaveChanges();
        }
    }
}
