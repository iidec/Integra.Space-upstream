//-----------------------------------------------------------------------
// <copyright file="PermissionOnSchemaFilter.cs" company="Integra.Space.Language">
//     Copyright (c) Integra.Space.Language. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using System.Linq;
    using Database;
    using Integra.Space.Language;
    using Ninject;

    /// <summary>
    /// Grant permission filter class.
    /// </summary>
    internal class PermissionOnSchemaFilter : PermissionFilter
    {
        /// <inheritdoc />
        protected override void SavePermissionForUser(SpaceDbContext databaseContext, Login login, Schema schemaOfPrincipal, Schema schemaOfSecurable, PermissionsCommandNode command, PermissionNode permission, DatabaseUser user)
        {
            SchemaAssignedPermissionsToUser newPermission = new SchemaAssignedPermissionsToUser();
            newPermission.SecurableClassId = databaseContext.SecurableClasses.Single(x => x.SecurableName.ToLower() == permission.CommandObject.SecurableClass.ToString().ToLower()).SecurableClassId;
            newPermission.GranularPermissionId = databaseContext.GranularPermissions.Single(x => x.GranularPermissionName.Replace(" ", string.Empty).ToLower() == permission.Permission.ToString().ToLower()).GranularPermissionId;
            newPermission.WithGrantOption = command.PermissionOption;
            newPermission.DatabaseUser = user;
            newPermission.Schema = databaseContext.Schemas.Single(x => x.ServerId == schemaOfSecurable.ServerId && x.DatabaseId == schemaOfSecurable.DatabaseId && x.SchemaName == permission.CommandObject.Name);
            
            Func<SchemaAssignedPermissionsToUser, bool> predicate = x => x.DbUsrServerId == newPermission.DatabaseUser.ServerId
                                                                             && x.DbUsrDatabaseId == newPermission.DatabaseUser.DatabaseId
                                                                             && x.DbUsrId == newPermission.DatabaseUser.DbUsrId
                                                                             && x.SchemaServerId == newPermission.Schema.ServerId
                                                                             && x.SchemaDatabaseId == newPermission.Schema.DatabaseId
                                                                             && x.SchemaId == newPermission.Schema.SchemaId
                                                                             && x.GranularPermissionId == newPermission.GranularPermissionId
                                                                             && x.SecurableClassId == newPermission.SecurableClassId;

            if (!databaseContext.SchemaAssignedPermissionsToUsers.Any(predicate))
            {
                if (command.Action == Common.ActionCommandEnum.Grant)
                {
                    newPermission.Granted = true;
                    databaseContext.SchemaAssignedPermissionsToUsers.Add(newPermission);
                }
                else if (command.Action == Common.ActionCommandEnum.Deny)
                {
                    newPermission.Denied = true;
                    databaseContext.SchemaAssignedPermissionsToUsers.Add(newPermission);
                }
                else
                {
                    throw new Exception("Only grant, deny or revoke command allowed.");
                }
            }
            else
            {
                SchemaAssignedPermissionsToUser permissionToUpdate = databaseContext.SchemaAssignedPermissionsToUsers.Single(predicate);
                if (command.Action == Common.ActionCommandEnum.Grant)
                {
                    permissionToUpdate.Granted = true;
                    permissionToUpdate.Denied = false;
                }
                else if (command.Action == Common.ActionCommandEnum.Deny)
                {
                    permissionToUpdate.Denied = true;
                }
                else if (command.Action == Common.ActionCommandEnum.Revoke)
                {
                    databaseContext.SchemaAssignedPermissionsToUsers.Remove(permissionToUpdate);
                }
                else
                {
                    throw new Exception("Only grant, deny or revoke command allowed.");
                }
            }

            databaseContext.SaveChanges();
        }

        /// <inheritdoc />
        protected override void SavePermissionForRole(SpaceDbContext databaseContext, Login login, Schema schemaOfPrincipal, Schema schemaOfSecurable, PermissionsCommandNode command, PermissionNode permission, DatabaseRole role)
        {
            SchemaAssignedPermissionsToDBRole newPermission = new SchemaAssignedPermissionsToDBRole();
            newPermission.SecurableClassId = databaseContext.SecurableClasses.Single(x => x.SecurableName.ToLower() == permission.CommandObject.SecurableClass.ToString().ToLower()).SecurableClassId;
            newPermission.GranularPermissionId = databaseContext.GranularPermissions.Single(x => x.GranularPermissionName.Replace(" ", string.Empty).ToLower() == permission.Permission.ToString().ToLower()).GranularPermissionId;
            newPermission.WithGrantOption = command.PermissionOption;
            newPermission.DatabaseRole = role;
            newPermission.Schema = databaseContext.Schemas.Single(x => x.ServerId == schemaOfSecurable.ServerId && x.DatabaseId == schemaOfSecurable.DatabaseId && x.SchemaName == permission.CommandObject.Name);

            Func<SchemaAssignedPermissionsToDBRole, bool> predicate = x => x.DbRoleServerId == newPermission.DatabaseRole.ServerId
                                                                             && x.DbRoleDatabaseId == newPermission.DatabaseRole.DatabaseId
                                                                             && x.DbRoleId == newPermission.DatabaseRole.DbRoleId
                                                                             && x.SchemaServerId == newPermission.Schema.ServerId
                                                                             && x.SchemaDatabaseId == newPermission.Schema.DatabaseId
                                                                             && x.SchemaId == newPermission.Schema.SchemaId
                                                                             && x.GranularPermissionId == newPermission.GranularPermissionId
                                                                             && x.SecurableClassId == newPermission.SecurableClassId;

            if (!databaseContext.SchemasAssignedPermissionsToDbRoles.Any(predicate))
            {
                if (command.Action == Common.ActionCommandEnum.Grant)
                {
                    newPermission.Granted = true;
                    databaseContext.SchemasAssignedPermissionsToDbRoles.Add(newPermission);
                }
                else if (command.Action == Common.ActionCommandEnum.Deny)
                {
                    newPermission.Denied = true;
                    databaseContext.SchemasAssignedPermissionsToDbRoles.Add(newPermission);
                }
                else
                {
                    throw new Exception("Only grant, deny or revoke command allowed.");
                }
            }
            else
            {
                SchemaAssignedPermissionsToDBRole permissionToUpdate = databaseContext.SchemasAssignedPermissionsToDbRoles.Single(predicate);
                if (command.Action == Common.ActionCommandEnum.Grant)
                {
                    permissionToUpdate.Granted = true;
                    permissionToUpdate.Denied = false;
                }
                else if (command.Action == Common.ActionCommandEnum.Deny)
                {
                    permissionToUpdate.Denied = true;
                }
                else if (command.Action == Common.ActionCommandEnum.Revoke)
                {
                    databaseContext.SchemasAssignedPermissionsToDbRoles.Remove(permissionToUpdate);
                }
                else
                {
                    throw new Exception("Only grant, deny or revoke command allowed.");
                }
            }

            databaseContext.SaveChanges();
        }

        /// <inheritdoc />
        protected override void SavePermissionForLogin(SpaceDbContext databaseContext, Login login, Schema schemaOfPrincipal, Schema schemaOfSecurable, PermissionsCommandNode command, PermissionNode permission, Login principal)
        {
            throw new NotImplementedException();
        }
    }
}
