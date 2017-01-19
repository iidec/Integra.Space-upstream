//-----------------------------------------------------------------------
// <copyright file="PermissionOnViewFilter.cs" company="Integra.Space.Language">
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
    internal class PermissionOnViewFilter : PermissionFilter
    {
        /// <inheritdoc />
        protected override void SavePermissionForUser(SpaceDbContext databaseContext, Login login, Schema schemaOfPrincipal, Schema schemaOfSecurable, PermissionsCommandNode command, PermissionNode permission, DatabaseUser user)
        {
            ViewAssignedPermissionsToUser newPermission = new ViewAssignedPermissionsToUser();
            newPermission.SecurableClassId = databaseContext.SecurableClasses.Single(x => x.SecurableName.Equals(permission.CommandObject.SecurableClass.ToString(), StringComparison.InvariantCultureIgnoreCase)).SecurableClassId;
            newPermission.GranularPermissionId = databaseContext.GranularPermissions.Single(x => x.GranularPermissionName.Replace(" ", string.Empty).Equals(permission.Permission.ToString(), StringComparison.InvariantCultureIgnoreCase)).GranularPermissionId;
            newPermission.WithGrantOption = command.PermissionOption;
            newPermission.DatabaseUser = user;

            newPermission.View = databaseContext.Views.Single(x => x.ServerId == schemaOfSecurable.ServerId && x.DatabaseId == schemaOfSecurable.DatabaseId && x.SchemaId == schemaOfSecurable.SchemaId && x.ViewName == permission.CommandObject.Name);
            
            Func<ViewAssignedPermissionsToUser, bool> predicate = x => x.DbUsrServerId == newPermission.DatabaseUser.ServerId
                                                                             && x.DbUsrDatabaseId == newPermission.DatabaseUser.DatabaseId
                                                                             && x.DbUsrId == newPermission.DatabaseUser.DbUsrId
                                                                             && x.ViewServerId == newPermission.View.ServerId
                                                                             && x.ViewDatabaseId == newPermission.View.DatabaseId
                                                                             && x.ViewSchemaId == newPermission.View.SchemaId
                                                                             && x.ViewId == newPermission.View.ViewId
                                                                             && x.GranularPermissionId == newPermission.GranularPermissionId
                                                                             && x.SecurableClassId == newPermission.SecurableClassId;

            if (!databaseContext.ViewAssignedPermissionsToUsers.Any(predicate))
            {
                if (command.Action == Common.ActionCommandEnum.Grant)
                {
                    newPermission.Granted = true;
                    databaseContext.ViewAssignedPermissionsToUsers.Add(newPermission);
                }
                else if (command.Action == Common.ActionCommandEnum.Deny)
                {
                    newPermission.Denied = true;
                    databaseContext.ViewAssignedPermissionsToUsers.Add(newPermission);
                }
                else
                {
                    throw new Exception("Only grant, deny or revoke command allowed.");
                }
            }
            else
            {
                ViewAssignedPermissionsToUser permissionToUpdate = databaseContext.ViewAssignedPermissionsToUsers.Single(predicate);
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
                    databaseContext.ViewAssignedPermissionsToUsers.Remove(permissionToUpdate);
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
            ViewAssignedPermissionsToDBRole newPermission = new ViewAssignedPermissionsToDBRole();
            newPermission.SecurableClassId = databaseContext.SecurableClasses.Single(x => x.SecurableName.Equals(permission.CommandObject.SecurableClass.ToString(), StringComparison.InvariantCultureIgnoreCase)).SecurableClassId;
            newPermission.GranularPermissionId = databaseContext.GranularPermissions.Single(x => x.GranularPermissionName.Replace(" ", string.Empty).Equals(permission.Permission.ToString(), StringComparison.InvariantCultureIgnoreCase)).GranularPermissionId;
            newPermission.WithGrantOption = command.PermissionOption;
            newPermission.DatabaseRole = role;
            newPermission.View = databaseContext.Views.Single(x => x.ServerId == schemaOfSecurable.ServerId && x.DatabaseId == schemaOfSecurable.DatabaseId && x.SchemaId == schemaOfSecurable.SchemaId && x.ViewName == permission.CommandObject.Name);

            Func<ViewAssignedPermissionsToDBRole, bool> predicate = x => x.DbRoleServerId == newPermission.DatabaseRole.ServerId
                                                                             && x.DbRoleDatabaseId == newPermission.DatabaseRole.DatabaseId
                                                                             && x.DbRoleId == newPermission.DatabaseRole.DbRoleId
                                                                             && x.ViewServerId == newPermission.View.ServerId
                                                                             && x.ViewDatabaseId == newPermission.View.DatabaseId
                                                                             && x.ViewSchemaId == newPermission.View.SchemaId
                                                                             && x.ViewId == newPermission.View.ViewId
                                                                             && x.GranularPermissionId == newPermission.GranularPermissionId
                                                                             && x.SecurableClassId == newPermission.SecurableClassId;

            if (!databaseContext.ViewAssignedPermissionsToDBRoles.Any(predicate))
            {
                if (command.Action == Common.ActionCommandEnum.Grant)
                {
                    newPermission.Granted = true;
                    databaseContext.ViewAssignedPermissionsToDBRoles.Add(newPermission);
                }
                else if (command.Action == Common.ActionCommandEnum.Deny)
                {
                    newPermission.Denied = true;
                    databaseContext.ViewAssignedPermissionsToDBRoles.Add(newPermission);
                }
                else
                {
                    throw new Exception("Only grant, deny or revoke command allowed.");
                }
            }
            else
            {
                ViewAssignedPermissionsToDBRole permissionToUpdate = databaseContext.ViewAssignedPermissionsToDBRoles.Single(predicate);
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
                    databaseContext.ViewAssignedPermissionsToDBRoles.Remove(permissionToUpdate);
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
