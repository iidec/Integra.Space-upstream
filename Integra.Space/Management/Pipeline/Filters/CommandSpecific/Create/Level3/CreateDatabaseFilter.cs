//-----------------------------------------------------------------------
// <copyright file="CreateDatabaseFilter.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database;

    /// <summary>
    /// Filter create source class.
    /// </summary>
    internal class CreateDatabaseFilter : CreateEntityFilter<Language.CreateDatabaseNode, Common.DatabaseOptionEnum>
    {
        /// <inheritdoc />
        protected override void CreateEntity(Language.CreateDatabaseNode command, Dictionary<Common.DatabaseOptionEnum, object> options, Login login, DatabaseUser user, Schema schema, SpaceDbContext databaseContext)
        {
            Database database = new Database();
            database.Server = schema.Database.Server;
            database.DatabaseId = Guid.NewGuid();
            database.DatabaseName = command.MainCommandObject.Name;

            // se le establece como propietario al login que esta creando la entidad
            database.OwnerServerId = login.ServerId;
            database.OwnerId = login.LoginId;

            database.IsActive = true;
            if (command.Options.ContainsKey(Common.DatabaseOptionEnum.Status))
            {
                database.IsActive = (bool)command.Options[Common.DatabaseOptionEnum.Status];
            }

            // almaceno la nueva entidad y guardo los cambios
            databaseContext.Databases.Add(database);
            databaseContext.SaveChanges();

            Schema newSchema = new Schema()
            {
                SchemaId = Guid.NewGuid(),
                SchemaName = "dbo",
                Database = database
            };
            
            databaseContext.Schemas.Add(newSchema);
            databaseContext.SaveChanges();

            Login sa = databaseContext.Logins.Single(l => l.ServerId == schema.ServerId && l.LoginName == "sa");

            DatabaseUser dboUser = new DatabaseUser()
            {
                DbUsrId = Guid.NewGuid(),
                DbUsrName = "dbo",
                Login = sa,
                IsActive = true,
                DefaultSchema = newSchema,
                Database = database
            };

            newSchema.DatabaseUser = dboUser;
            databaseContext.DatabaseUsers.Add(dboUser);
            databaseContext.SaveChanges();

            SecurableClass securableClass = databaseContext.SecurableClasses.Single(x => x.SecurableName.Equals("database", StringComparison.InvariantCultureIgnoreCase));
            GranularPermission granularPermission = databaseContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("control", StringComparison.InvariantCultureIgnoreCase));
            DatabaseAssignedPermissionsToUser newPermissionForDBO = new DatabaseAssignedPermissionsToUser()
            {
                Database = database,
                DatabaseUser = dboUser,
                GranularPermissionId = granularPermission.GranularPermissionId,
                SecurableClassId = securableClass.SecurableClassId,
                Granted = true,
                WithGrantOption = true
            };

            databaseContext.DatabaseAssignedPermissionsToUsers.Add(newPermissionForDBO);
            databaseContext.SaveChanges();

            DatabaseUser userForTheLogin = new DatabaseUser()
            {
                DbUsrId = Guid.NewGuid(),
                DbUsrName = login.LoginName,
                Login = login,
                IsActive = true,
                DefaultSchema = newSchema,
                Database = database
            };

            databaseContext.DatabaseUsers.Add(userForTheLogin);
            databaseContext.SaveChanges();

            DatabaseAssignedPermissionsToUser newPermissionForUserLogin = new DatabaseAssignedPermissionsToUser()
            {
                Database = database,
                DatabaseUser = userForTheLogin,
                GranularPermissionId = granularPermission.GranularPermissionId,
                SecurableClassId = securableClass.SecurableClassId,
                Granted = true,
                WithGrantOption = true
            };

            databaseContext.DatabaseAssignedPermissionsToUsers.Add(newPermissionForUserLogin);
            databaseContext.SaveChanges();
        }
    }
}
