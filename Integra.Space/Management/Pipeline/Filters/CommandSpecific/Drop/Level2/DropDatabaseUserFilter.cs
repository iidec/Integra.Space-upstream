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
        protected override void DropEntity(SpaceDbContext databaseContext, Schema schema, string name)
        {
            DatabaseUser databaseUser = databaseContext.DatabaseUsers.Single(x => x.ServerId == schema.ServerId
                                            && x.DatabaseId == schema.DatabaseId
                                            && x.DbUsrName == name);

            databaseContext.DatabaseUsers.Remove(databaseUser);
            databaseContext.SaveChanges();
        }
    }
}
