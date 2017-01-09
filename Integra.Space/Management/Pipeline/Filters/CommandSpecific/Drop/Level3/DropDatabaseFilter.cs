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
        protected override void DropEntity(SpaceDbContext databaseContext, Schema schema, string name)
        {
            Database database = databaseContext.Databases.Single(x => x.ServerId == schema.ServerId
                                            && x.DatabaseName == name);

            database.DatabaseUsers.ToList().ForEach(x => databaseContext.DatabaseUsers.Remove(x));
            databaseContext.SaveChanges();
            databaseContext.Databases.Remove(database);
            databaseContext.SaveChanges();
        }
    }
}
