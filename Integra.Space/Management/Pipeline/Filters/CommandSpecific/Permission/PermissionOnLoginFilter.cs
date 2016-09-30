//-----------------------------------------------------------------------
// <copyright file="PermissionOnLoginFilter.cs" company="Integra.Space.Language">
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
    internal class PermissionOnLoginFilter : PermissionFilter
    {
        /// <inheritdoc />
        protected override void SavePermissionForUser(CommandObject principal, PipelineContext context, Schema schema, PermissionsCommandNode command, PermissionNode permission)
        {
            SpaceDbContext databaseContext = context.Kernel.Get<SpaceDbContext>();
            Login login = databaseContext.Logins.Single((System.Linq.Expressions.Expression<Func<Login, bool>>)(x => (bool)(x.ServerId == schema.Database.Server.ServerId && x.LoginName == principal.Name)));

            LoginAssignedPermissionsToLogin newPermission = new LoginAssignedPermissionsToLogin();
            newPermission.SecurableClassId = databaseContext.SecurableClasses.Single(x => x.SecurableName.Equals(permission.ObjectType.ToString(), StringComparison.InvariantCultureIgnoreCase)).SecurableClassId;
            newPermission.GranularPermissionId = databaseContext.GranularPermissions.Single(x => x.GranularPermissionName.Replace(" ", string.Empty).Equals(permission.Permission.ToString(), StringComparison.InvariantCultureIgnoreCase)).GranularPermissionId;
            newPermission.WithGrantOption = command.PermissionOption;
            newPermission.Login = login;
            newPermission.Login = databaseContext.Logins.Single(x => x.ServerId == schema.ServerId && x.LoginName == permission.ObjectName);

            Func<LoginAssignedPermissionsToLogin, bool> predicate = x => x.LoginServerId == newPermission.Login.ServerId
                                                                             && x.LoginId == newPermission.Login.LoginId
                                                                             && x.OnLoginServerId == newPermission.Login.ServerId
                                                                             && x.OnLoginId == newPermission.Login.LoginId
                                                                             && x.GranularPermissionId == newPermission.GranularPermissionId
                                                                             && x.SecurableClassId == newPermission.SecurableClassId;

            if (!databaseContext.LoginsAssignedPermissionsToLogins.Any(predicate))
            {
                if (command.Action == Common.ActionCommandEnum.Grant)
                {
                    newPermission.Granted = true;
                    databaseContext.LoginsAssignedPermissionsToLogins.Add(newPermission);
                }
                else if (command.Action == Common.ActionCommandEnum.Deny)
                {
                    newPermission.Denied = true;
                    databaseContext.LoginsAssignedPermissionsToLogins.Add(newPermission);
                }
                else
                {
                    throw new Exception("Only grant, deny or revoke command allowed.");
                }
            }
            else
            {
                LoginAssignedPermissionsToLogin permissionToUpdate = databaseContext.LoginsAssignedPermissionsToLogins.Single(predicate);
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
                    databaseContext.LoginsAssignedPermissionsToLogins.Remove(permissionToUpdate);
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
            throw new InvalidOperationException("Cannot assign a login permission to a role.");
        }
    }
}
