//-----------------------------------------------------------------------
// <copyright file="PermissionOnEndpointFilter.cs" company="Integra.Space.Language">
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
    internal class PermissionOnEndpointFilter : PermissionFilter
    {
        /// <inheritdoc />
        protected override void SavePermissionForUser(CommandObject principal, PipelineContext context, Schema schema, PermissionsCommandNode command, PermissionNode permission)
        {
            SpaceDbContext databaseContext = context.Kernel.Get<SpaceDbContext>();
            Login login = databaseContext.Logins.Single((System.Linq.Expressions.Expression<Func<Login, bool>>)(x => (bool)(x.ServerId == schema.Database.Server.ServerId && x.LoginName == principal.Name)));

            EndpointAssignedPermissionsToLogin newPermission = new EndpointAssignedPermissionsToLogin();
            newPermission.SecurableClassId = databaseContext.SecurableClasses.Single(x => x.SecurableName.Equals(permission.ObjectType.ToString(), StringComparison.InvariantCultureIgnoreCase)).SecurableClassId;
            newPermission.GranularPermissionId = databaseContext.GranularPermissions.Single(x => x.GranularPermissionName.Replace(" ", string.Empty).Equals(permission.Permission.ToString(), StringComparison.InvariantCultureIgnoreCase)).GranularPermissionId;
            newPermission.WithGrantOption = command.PermissionOption;
            newPermission.Login = login;
            newPermission.Endpoint = databaseContext.Endpoints.Single(x => x.ServerId == schema.ServerId && x.EnpointName == permission.ObjectName);

            Func<EndpointAssignedPermissionsToLogin, bool> predicate = x => x.LoginServerId == newPermission.Login.ServerId
                                                                             && x.LoginId == newPermission.Login.LoginId
                                                                             && x.EndpointServerId == newPermission.Endpoint.ServerId
                                                                             && x.EndpointId == newPermission.Endpoint.EndpointId
                                                                             && x.GranularPermissionId == newPermission.GranularPermissionId
                                                                             && x.SecurableClassId == newPermission.SecurableClassId;

            if (!databaseContext.EndpointsAssignedPermissionsToLogins.Any(predicate))
            {
                if (command.Action == Common.ActionCommandEnum.Grant)
                {
                    newPermission.Granted = true;
                    databaseContext.EndpointsAssignedPermissionsToLogins.Add(newPermission);
                }
                else if (command.Action == Common.ActionCommandEnum.Deny)
                {
                    newPermission.Denied = true;
                    databaseContext.EndpointsAssignedPermissionsToLogins.Add(newPermission);
                }
                else
                {
                    throw new Exception("Only grant, deny or revoke command allowed.");
                }
            }
            else
            {
                EndpointAssignedPermissionsToLogin permissionToUpdate = databaseContext.EndpointsAssignedPermissionsToLogins.Single(predicate);
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
                    databaseContext.EndpointsAssignedPermissionsToLogins.Remove(permissionToUpdate);
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
            throw new InvalidOperationException("Cannot assign an endpoint permission to a role.");
        }
    }
}
