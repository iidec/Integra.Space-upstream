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

    /// <summary>
    /// Grant permission filter class.
    /// </summary>
    internal class PermissionOnLoginFilter : PermissionFilter
    {
        /// <inheritdoc />
        protected override void SavePermissionForUser(SpaceDbContext databaseContext, Login login, Schema schemaOfPrincipal, Schema schemaOfSecurable, PermissionsCommandNode command, PermissionNode permission, DatabaseUser user)
        {
            throw new InvalidOperationException("Cannot assign a login permission to a database user.");
        }

        /// <inheritdoc />
        protected override void SavePermissionForRole(SpaceDbContext databaseContext, Login login, Schema schemaOfPrincipal, Schema schemaOfSecurable, PermissionsCommandNode command, PermissionNode permission, DatabaseRole role)
        {
            throw new InvalidOperationException("Cannot assign a login permission to a database role.");
        }

        /// <inheritdoc />
        protected override void SavePermissionForLogin(SpaceDbContext databaseContext, Login login, Schema schemaOfPrincipal, Schema schemaOfSecurable, PermissionsCommandNode command, PermissionNode permission, Login principal)
        {
            LoginAssignedPermissionsToLogin newPermission = new LoginAssignedPermissionsToLogin();
            newPermission.SecurableClassId = databaseContext.SecurableClasses.Single(x => x.SecurableName.Equals(permission.CommandObject.SecurableClass.ToString(), StringComparison.InvariantCultureIgnoreCase)).SecurableClassId;
            newPermission.GranularPermissionId = databaseContext.GranularPermissions.Single(x => x.GranularPermissionName.Replace(" ", string.Empty).Equals(permission.Permission.ToString(), StringComparison.InvariantCultureIgnoreCase)).GranularPermissionId;
            newPermission.WithGrantOption = command.PermissionOption;
            newPermission.Login = principal;
            newPermission.Login = databaseContext.Logins.Single(x => x.ServerId == schemaOfSecurable.ServerId && x.LoginName == permission.CommandObject.Name);

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
    }
}
