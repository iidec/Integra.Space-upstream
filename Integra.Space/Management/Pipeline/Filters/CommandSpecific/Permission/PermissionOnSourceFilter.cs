//-----------------------------------------------------------------------
// <copyright file="PermissionOnSourceFilter.cs" company="Integra.Space.Language">
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
    internal class PermissionOnSourceFilter : PermissionFilter
    {
        /// <inheritdoc />
        protected override void SavePermissionForUser(SpaceDbContext databaseContext, Login login, Schema schemaOfPrincipal, Schema schemaOfSecurable, PermissionsCommandNode command, PermissionNode permission, DatabaseUser user)
        {
            SourceAssignedPermissionsToUser newPermission = new SourceAssignedPermissionsToUser();
            newPermission.SecurableClassId = databaseContext.SecurableClasses.Single(x => x.SecurableName.ToLower() == permission.CommandObject.SecurableClass.ToString().ToLower()).SecurableClassId;
            newPermission.GranularPermissionId = databaseContext.GranularPermissions.Single(x => x.GranularPermissionName.Replace(" ", string.Empty).ToLower() == permission.Permission.ToString().ToLower()).GranularPermissionId;
            newPermission.WithGrantOption = command.PermissionOption;
            newPermission.DatabaseUser = user;
            newPermission.Source = databaseContext.Sources.Single(x => x.ServerId == schemaOfSecurable.ServerId && x.DatabaseId == schemaOfSecurable.DatabaseId && x.SchemaId == schemaOfSecurable.SchemaId && x.SourceName == permission.CommandObject.Name);
            
            Func<SourceAssignedPermissionsToUser, bool> predicate = x => x.DbUsrServerId == newPermission.DatabaseUser.ServerId
                                                                             && x.DbUserDatabaseId == newPermission.DatabaseUser.DatabaseId
                                                                             && x.DbUserId == newPermission.DatabaseUser.DbUsrId
                                                                             && x.SourceServerId == newPermission.Source.ServerId
                                                                             && x.SourceDatabaseId == newPermission.Source.DatabaseId
                                                                             && x.SourceSchemaId == newPermission.Source.SchemaId
                                                                             && x.SourceId == newPermission.Source.SourceId
                                                                             && x.GranularPermissionId == newPermission.GranularPermissionId
                                                                             && x.SecurableClassId == newPermission.SecurableClassId;

            if (!databaseContext.SourceAssignedPermissionsToUsers.Any(predicate))
            {
                if (command.Action == Common.ActionCommandEnum.Grant)
                {
                    newPermission.Granted = true;
                    databaseContext.SourceAssignedPermissionsToUsers.Add(newPermission);
                }
                else if (command.Action == Common.ActionCommandEnum.Deny)
                {
                    newPermission.Denied = true;
                    databaseContext.SourceAssignedPermissionsToUsers.Add(newPermission);
                }
                else
                {
                    throw new Exception("Only grant, deny or revoke command allowed.");
                }
            }
            else
            {
                SourceAssignedPermissionsToUser permissionToUpdate = databaseContext.SourceAssignedPermissionsToUsers.Single(predicate);
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
                    databaseContext.SourceAssignedPermissionsToUsers.Remove(permissionToUpdate);
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
            SourceAssignedPermissionsToDBRole newPermission = new SourceAssignedPermissionsToDBRole();
            newPermission.SecurableClassId = databaseContext.SecurableClasses.Single(x => x.SecurableName.ToLower() == permission.CommandObject.SecurableClass.ToString().ToLower()).SecurableClassId;
            newPermission.GranularPermissionId = databaseContext.GranularPermissions.Single(x => x.GranularPermissionName.Replace(" ", string.Empty).ToLower() == permission.Permission.ToString().ToLower()).GranularPermissionId;
            newPermission.WithGrantOption = command.PermissionOption;
            newPermission.DatabaseRole = role;
            newPermission.Source = databaseContext.Sources.Single(x => x.ServerId == schemaOfSecurable.ServerId && x.DatabaseId == schemaOfSecurable.DatabaseId && x.SchemaId == schemaOfSecurable.SchemaId && x.SourceName == permission.CommandObject.Name);

            Func<SourceAssignedPermissionsToDBRole, bool> predicate = x => x.DbRoleServerId == newPermission.DatabaseRole.ServerId
                                                                             && x.DbRoleDatabaseId == newPermission.DatabaseRole.DatabaseId
                                                                             && x.DbRoleId == newPermission.DatabaseRole.DbRoleId
                                                                             && x.SourceServerId == newPermission.Source.ServerId
                                                                             && x.SourceDatabaseId == newPermission.Source.DatabaseId
                                                                             && x.SourceSchemaId == newPermission.Source.SchemaId
                                                                             && x.SourceId == newPermission.Source.SourceId
                                                                             && x.GranularPermissionId == newPermission.GranularPermissionId
                                                                             && x.SecurableClassId == newPermission.SecurableClassId;

            if (!databaseContext.SourceAssignedPermissionsToDBRoles.Any(predicate))
            {
                if (command.Action == Common.ActionCommandEnum.Grant)
                {
                    newPermission.Granted = true;
                    databaseContext.SourceAssignedPermissionsToDBRoles.Add(newPermission);
                }
                else if (command.Action == Common.ActionCommandEnum.Deny)
                {
                    newPermission.Denied = true;
                    databaseContext.SourceAssignedPermissionsToDBRoles.Add(newPermission);
                }
                else
                {
                    throw new Exception("Only grant, deny or revoke command allowed.");
                }
            }
            else
            {
                SourceAssignedPermissionsToDBRole permissionToUpdate = databaseContext.SourceAssignedPermissionsToDBRoles.Single(predicate);
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
                    databaseContext.SourceAssignedPermissionsToDBRoles.Remove(permissionToUpdate);
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
