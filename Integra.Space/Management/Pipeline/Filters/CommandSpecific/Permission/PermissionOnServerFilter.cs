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
        protected override void SavePermissionForUser(CommandObject principal, PipelineContext context, Schema schema, PermissionsCommandNode command, PermissionNode permission)
        {
            SpaceDbContext databaseContext = context.Kernel.Get<SpaceDbContext>();
            Login login = databaseContext.Logins.Single(x => x.ServerId == schema.Database.Server.ServerId && x.LoginName == principal.Name);

            ServerAssignedPermissionsToLogin newPermission = new ServerAssignedPermissionsToLogin();
            newPermission.Login = login;
            newPermission.SecurableClassId = databaseContext.SecurableClasses.Single(x => x.SecurableName.Equals(permission.ObjectType.ToString(), StringComparison.InvariantCultureIgnoreCase)).SecurableClassId;
            newPermission.GranularPermissionId = databaseContext.GranularPermissions.Single(x => x.GranularPermissionName.Replace(" ", string.Empty).Equals(permission.Permission.ToString(), StringComparison.InvariantCultureIgnoreCase)).GranularPermissionId;
            newPermission.WithGrantOption = command.PermissionOption;

            if (permission.ObjectName == null)
            {
                command.Permission.ObjectName = context.CommandContext.Schema.Database.DatabaseName;
                newPermission.Server = context.CommandContext.Schema.Database.Server;
            }
            else
            {
                newPermission.Server = databaseContext.Servers.Single(x => x.ServerId == schema.ServerId);
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

        /// <inheritdoc />
        protected override void SavePermissionForRole(CommandObject role, PipelineContext context, Schema schema, PermissionsCommandNode command, PermissionNode permission)
        {
            throw new InvalidOperationException("Cannot assign a server permission to a role.");
        }
    }
}
