//-----------------------------------------------------------------------
// <copyright file="PermissionOnServerFilter.cs" company="Integra.Space.Language">
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
    internal class PermissionOnServerFilter : PermissionFilter
    {
        /// <inheritdoc />
        protected override void SavePermissionForUser(SpaceDbContext databaseContext, Login login, Schema schemaOfPrincipal, Schema schemaOfSecurable, PermissionsCommandNode command, PermissionNode permission, DatabaseUser user)
        {
            throw new InvalidOperationException("Cannot assign a server permission to a database user.");
        }

        /// <inheritdoc />
        protected override void SavePermissionForRole(SpaceDbContext databaseContext, Login login, Schema schemaOfPrincipal, Schema schemaOfSecurable, PermissionsCommandNode command, PermissionNode permission, DatabaseRole role)
        {
            throw new InvalidOperationException("Cannot assign a server permission to a database role.");
        }

        /// <inheritdoc />
        protected override void SavePermissionForLogin(SpaceDbContext databaseContext, Login login, Schema schemaOfPrincipal, Schema schemaOfSecurable, PermissionsCommandNode command, PermissionNode permission, Login principal)
        {
            ServerAssignedPermissionsToLogin newPermission = new ServerAssignedPermissionsToLogin();
            newPermission.Login = principal;
            newPermission.SecurableClassId = databaseContext.SecurableClasses.Single(x => x.SecurableName.ToLower() == permission.CommandObject.SecurableClass.ToString().ToLower()).SecurableClassId;
            newPermission.GranularPermissionId = databaseContext.GranularPermissions.Single(x => x.GranularPermissionName.Replace(" ", string.Empty).ToLower() == permission.Permission.ToString().ToLower()).GranularPermissionId;
            newPermission.WithGrantOption = command.PermissionOption;

            if (permission.CommandObject == null)
            {
                // command.Permission.ObjectName = login.Database.Server.ServerName; // context.CommandContext.Schema.Database.DatabaseName;
                newPermission.Server = schemaOfSecurable.Database.Server; // context.CommandContext.Schema.Database.Server;
            }
            else
            {
                newPermission.Server = databaseContext.Servers.Single(x => x.ServerId == schemaOfSecurable.ServerId);
            }

            Func<ServerAssignedPermissionsToLogin, bool> predicate = x => x.LoginServerId == newPermission.Login.ServerId
                                                                             && x.LoginId == newPermission.Login.LoginId
                                                                             && x.ServerId == newPermission.Server.ServerId
                                                                             && x.GranularPermissionId == newPermission.GranularPermissionId
                                                                             && x.SecurableClassId == newPermission.SecurableClassId;

            if (!databaseContext.ServersAssignedPermissionsToLogins.Any(predicate))
            {
                if (command.Action == Common.ActionCommandEnum.Grant)
                {
                    newPermission.Granted = true;
                    databaseContext.ServersAssignedPermissionsToLogins.Add(newPermission);
                }
                else if (command.Action == Common.ActionCommandEnum.Deny)
                {
                    newPermission.Denied = true;
                    databaseContext.ServersAssignedPermissionsToLogins.Add(newPermission);
                }
                else
                {
                    throw new Exception("Only grant, deny or revoke command allowed.");
                }
            }
            else
            {
                ServerAssignedPermissionsToLogin permissionToUpdate = databaseContext.ServersAssignedPermissionsToLogins.Single(predicate);
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
                    databaseContext.ServersAssignedPermissionsToLogins.Remove(permissionToUpdate);
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
