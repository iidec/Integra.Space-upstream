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
            Database database = databaseContext.Databases.Single(x => x.ServerId == schema.ServerId && x.DatabaseName == name);
            Schema dboSchema = database.Schemas.Single(x => x.SchemaName == DatabaseConstants.DBO_SCHEMA_NAME);
            DatabaseUser dboUser = database.DatabaseUsers.Single(x => x.DbUsrName == DatabaseConstants.DBO_USER_NAME);

            databaseContext.DatabaseAssignedPermissionsToUsers.RemoveRange(databaseContext.DatabaseAssignedPermissionsToUsers.Where(x => x.DbUsrServerId == dboUser.ServerId && x.DbUsrDatabaseId == dboUser.DatabaseId && x.DbUsrId == dboUser.DbUsrId));
            databaseContext.DBRolesAssignedPermissionsToUsers.RemoveRange(databaseContext.DBRolesAssignedPermissionsToUsers.Where(x => x.DbUsrServerId == dboUser.ServerId && x.DbUsrDatabaseId == dboUser.DatabaseId && x.DbUsrId == dboUser.DbUsrId));
            databaseContext.SchemaAssignedPermissionsToUsers.RemoveRange(databaseContext.SchemaAssignedPermissionsToUsers.Where(x => x.DbUsrServerId == dboUser.ServerId && x.DbUsrDatabaseId == dboUser.DatabaseId && x.DbUsrId == dboUser.DbUsrId));
            databaseContext.SourceAssignedPermissionsToUsers.RemoveRange(databaseContext.SourceAssignedPermissionsToUsers.Where(x => x.DbUsrServerId == dboUser.ServerId && x.DbUserDatabaseId == dboUser.DatabaseId && x.DbUserId == dboUser.DbUsrId));
            databaseContext.StreamAssignedPermissionsToUsers.RemoveRange(databaseContext.StreamAssignedPermissionsToUsers.Where(x => x.DbUsrServerId == dboUser.ServerId && x.DbUsrDatabaseId == dboUser.DatabaseId && x.DbUsrId == dboUser.DbUsrId));
            databaseContext.UserAssignedPermissionsToDBRoles.RemoveRange(databaseContext.UserAssignedPermissionsToDBRoles.Where(x => x.DbUsrServerId == dboUser.ServerId && x.DbUsrDatabaseId == dboUser.DatabaseId && x.DbUsrId == dboUser.DbUsrId));
            databaseContext.UserAssignedPermissionsToUsers.RemoveRange(databaseContext.UserAssignedPermissionsToUsers.Where(x => x.DbUsrServerId == dboUser.ServerId && x.DbUsrDatabaseId == dboUser.DatabaseId && x.DbUsrId == dboUser.DbUsrId));
            databaseContext.ViewAssignedPermissionsToUsers.RemoveRange(databaseContext.ViewAssignedPermissionsToUsers.Where(x => x.DbUsrServerId == dboUser.ServerId && x.DbUsrDatabaseId == dboUser.DatabaseId && x.DbUsrId == dboUser.DbUsrId));
            databaseContext.SaveChanges();
            
            SpaceEnvironment env = new SpaceEnvironment();
            env.ExecuteCommandsBeforeCreateDatabase(databaseContext);

            dboSchema.OwnerServerId = System.Guid.Empty;
            dboSchema.OwnerDatabaseId = System.Guid.Empty;
            dboSchema.OwnerId = System.Guid.Empty;
            dboSchema.DatabaseUser = null;
            databaseContext.DatabaseUsers.Remove(dboUser);
            databaseContext.SaveChanges();

            databaseContext.Schemas.Remove(dboSchema);
            databaseContext.SaveChanges();
            
            env.ExecuteCommandsAfterCreateDatabase(databaseContext);

            databaseContext.Databases.Remove(database);
            databaseContext.SaveChanges();
        }
    }
}
