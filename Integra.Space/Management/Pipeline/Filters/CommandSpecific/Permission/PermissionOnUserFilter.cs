//-----------------------------------------------------------------------
// <copyright file="PermissionOnUserFilter.cs" company="Integra.Space.Language">
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
    internal class PermissionOnUserFilter : PermissionFilter
    {
        /// <inheritdoc />
        protected override void SavePermissionForUser(SpaceDbContext databaseContext, Login login, Schema schemaOfPrincipal, Schema schemaOfSecurable, PermissionsCommandNode command, PermissionNode permission, DatabaseUser user)
        {
            UserAssignedPermissionsToUsers newPermission = new UserAssignedPermissionsToUsers();
            newPermission.SecurableClassId = databaseContext.SecurableClasses.Single(x => x.SecurableName.ToLower() == permission.CommandObject.SecurableClass.ToString().ToLower()).SecurableClassId;
            newPermission.GranularPermissionId = databaseContext.GranularPermissions.Single(x => x.GranularPermissionName.Replace(" ", string.Empty).ToLower() == permission.Permission.ToString().ToLower()).GranularPermissionId;
            newPermission.WithGrantOption = command.PermissionOption;
            newPermission.DatabaseUser = user;
            newPermission.DatabaseUser1 = databaseContext.DatabaseUsers.Single(x => x.ServerId == schemaOfSecurable.ServerId && x.DatabaseId == schemaOfSecurable.DatabaseId && x.DbUsrName == permission.CommandObject.Name);

            Func<UserAssignedPermissionsToUsers, bool> predicate = x => x.DbUsrServerId == newPermission.DatabaseUser.ServerId
                                                                             && x.DbUsrDatabaseId == newPermission.DatabaseUser.DatabaseId
                                                                             && x.DbUsrId == newPermission.DatabaseUser.DbUsrId
                                                                             && x.OnDbUsrServerId == newPermission.DatabaseUser1.ServerId
                                                                             && x.OnDbUsrDatabaseId == newPermission.DatabaseUser1.DatabaseId
                                                                             && x.OnDbUsrId == newPermission.DatabaseUser1.DbUsrId
                                                                             && x.GranularPermissionId == newPermission.GranularPermissionId
                                                                             && x.SecurableClassId == newPermission.SecurableClassId;

            if (!databaseContext.UserAssignedPermissionsToUsers.Any(predicate))
            {
                if (command.Action == Common.ActionCommandEnum.Grant)
                {
                    newPermission.Granted = true;
                    databaseContext.UserAssignedPermissionsToUsers.Add(newPermission);
                }
                else if (command.Action == Common.ActionCommandEnum.Deny)
                {
                    newPermission.Denied = true;
                    databaseContext.UserAssignedPermissionsToUsers.Add(newPermission);
                }
                else
                {
                    throw new Exception("Only grant, deny or revoke command allowed.");
                }
            }
            else
            {
                UserAssignedPermissionsToUsers permissionToUpdate = databaseContext.UserAssignedPermissionsToUsers.Single(predicate);
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
                    databaseContext.UserAssignedPermissionsToUsers.Remove(permissionToUpdate);
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
            UserAssignedPermissionsToDBRole newPermission = new UserAssignedPermissionsToDBRole();
            newPermission.SecurableClassId = databaseContext.SecurableClasses.Single(x => x.SecurableName.ToLower() == permission.CommandObject.SecurableClass.ToString().ToLower()).SecurableClassId;
            newPermission.GranularPermissionId = databaseContext.GranularPermissions.Single(x => x.GranularPermissionName.Replace(" ", string.Empty).ToLower() == permission.Permission.ToString().ToLower()).GranularPermissionId;
            newPermission.WithGrantOption = command.PermissionOption;
            newPermission.DatabaseRole = role;
            newPermission.DatabaseUser = databaseContext.DatabaseUsers.Single(x => x.ServerId == schemaOfSecurable.ServerId && x.DatabaseId == schemaOfSecurable.DatabaseId && x.DbUsrName == permission.CommandObject.Name);

            Func<UserAssignedPermissionsToDBRole, bool> predicate = x => x.DbRoleServerId == newPermission.DatabaseRole.ServerId
                                                                             && x.DbRoleDatabaseId == newPermission.DatabaseRole.DatabaseId
                                                                             && x.DbRoleId == newPermission.DatabaseRole.DbRoleId
                                                                             && x.DbUsrServerId == newPermission.DatabaseUser.ServerId
                                                                             && x.DbUsrDatabaseId == newPermission.DatabaseUser.DatabaseId
                                                                             && x.DbUsrId == newPermission.DatabaseUser.DbUsrId
                                                                             && x.GranularPermissionId == newPermission.GranularPermissionId
                                                                             && x.SecurableClassId == newPermission.SecurableClassId;

            if (!databaseContext.UserAssignedPermissionsToDBRoles.Any(predicate))
            {
                if (command.Action == Common.ActionCommandEnum.Grant)
                {
                    newPermission.Granted = true;
                    databaseContext.UserAssignedPermissionsToDBRoles.Add(newPermission);
                }
                else if (command.Action == Common.ActionCommandEnum.Deny)
                {
                    newPermission.Denied = true;
                    databaseContext.UserAssignedPermissionsToDBRoles.Add(newPermission);
                }
                else
                {
                    throw new Exception("Only grant, deny or revoke command allowed.");
                }
            }
            else
            {
                UserAssignedPermissionsToDBRole permissionToUpdate = databaseContext.UserAssignedPermissionsToDBRoles.Single(predicate);
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
                    databaseContext.UserAssignedPermissionsToDBRoles.Remove(permissionToUpdate);
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
