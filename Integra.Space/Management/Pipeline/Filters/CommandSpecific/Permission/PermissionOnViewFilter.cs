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
        protected override void SavePermissionForUser(CommandObject user, PipelineContext context, Schema schema, PermissionsCommandNode command, PermissionNode permission)
        {
            SpaceDbContext databaseContext = context.Kernel.Get<SpaceDbContext>();
            DatabaseUser databaseUser = databaseContext.DatabaseUsers.Single(x => x.ServerId == schema.Database.Server.ServerId && x.DatabaseId == schema.DatabaseId && x.DbUsrName == user.Name);

            ViewAssignedPermissionsToUser newPermission = new ViewAssignedPermissionsToUser();
            newPermission.SecurableClassId = databaseContext.SecurableClasses.Single(x => x.SecurableName.Equals(permission.ObjectType.ToString(), StringComparison.InvariantCultureIgnoreCase)).SecurableClassId;
            newPermission.GranularPermissionId = databaseContext.GranularPermissions.Single(x => x.GranularPermissionName.Replace(" ", string.Empty).Equals(permission.Permission.ToString(), StringComparison.InvariantCultureIgnoreCase)).GranularPermissionId;
            newPermission.WithGrantOption = command.PermissionOption;
            newPermission.DatabaseUser = databaseUser;
            newPermission.View = databaseContext.Views.Single(x => x.ServerId == schema.ServerId && x.DatabaseId == schema.DatabaseId && x.SchemaId == schema.SchemaId && x.ViewName == permission.ObjectName);
            
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
        protected override void SavePermissionForRole(CommandObject role, PipelineContext context, Schema schema, PermissionsCommandNode command, PermissionNode permission)
        {
            SpaceDbContext databaseContext = context.Kernel.Get<SpaceDbContext>();
            DatabaseRole databaseRole = databaseContext.DatabaseRoles.Single(x => x.ServerId == schema.Database.Server.ServerId && x.DatabaseId == schema.DatabaseId && x.DbRoleName == role.Name);

            ViewAssignedPermissionsToDBRole newPermission = new ViewAssignedPermissionsToDBRole();
            newPermission.SecurableClassId = databaseContext.SecurableClasses.Single(x => x.SecurableName.Equals(permission.ObjectType.ToString(), StringComparison.InvariantCultureIgnoreCase)).SecurableClassId;
            newPermission.GranularPermissionId = databaseContext.GranularPermissions.Single(x => x.GranularPermissionName.Replace(" ", string.Empty).Equals(permission.Permission.ToString(), StringComparison.InvariantCultureIgnoreCase)).GranularPermissionId;
            newPermission.WithGrantOption = command.PermissionOption;
            newPermission.DatabaseRole = databaseRole;
            newPermission.View = databaseContext.Views.Single(x => x.ServerId == schema.ServerId && x.DatabaseId == schema.DatabaseId && x.SchemaId == schema.SchemaId && x.ViewName == permission.ObjectName);

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
    }
}
