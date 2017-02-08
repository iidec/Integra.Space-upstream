//-----------------------------------------------------------------------
// <copyright file="PermissionFilter.cs" company="Integra.Space.Language">
//     Copyright (c) Integra.Space.Language. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using System.Linq;
    using Common;
    using Database;
    using Integra.Space.Language;
    using Ninject;

    /// <summary>
    /// Grant permission filter class.
    /// </summary>
    internal abstract class PermissionFilter : CommandFilter
    {
        /// <inheritdoc />
        public override PipelineContext Execute(PipelineContext context)
        {
            SpaceDbContext databaseContext = context.Kernel.Get<SpaceDbContext>();
            Login login = context.SecurityContext.Login;
            PermissionsCommandNode command = (PermissionsCommandNode)context.CommandContext.Command;
            PermissionNode permission = command.Permission;
            Schema schemaOfSecurable = permission.CommandObject.GetSchema(databaseContext, login);

            DatabaseAssignedPermissionsToUser p1 = new DatabaseAssignedPermissionsToUser();
            foreach (CommandObject principal in command.Principals)
            {
                SystemObjectEnum permissionObjectType = permission.CommandObject.SecurableClass;

                Schema schemaOfPrincipal = principal.GetSchema(databaseContext, login);
                if (principal.SecurableClass == SystemObjectEnum.DatabaseUser)
                {
                    if (permissionObjectType == SystemObjectEnum.Server || permissionObjectType == SystemObjectEnum.Endpoint || permissionObjectType == SystemObjectEnum.Login)
                    {
                        throw new Exception(string.Format("Invalid principal for the permission '{0}' for object type '{1}'.", command.Permission.Permission, permissionObjectType));
                    }
                    else
                    {
                        DatabaseUser databaseUser = databaseContext.DatabaseUsers.Single(x => x.ServerId == schemaOfPrincipal.Database.Server.ServerId && x.DatabaseId == schemaOfPrincipal.DatabaseId && x.DbUsrName == principal.Name);
                        this.SavePermissionForUser(databaseContext, login, schemaOfPrincipal, schemaOfSecurable, command, permission, databaseUser);
                    }
                }
                else if (principal.SecurableClass == SystemObjectEnum.Login)
                {
                    if (permissionObjectType == SystemObjectEnum.Server || permissionObjectType == SystemObjectEnum.Endpoint || permissionObjectType == SystemObjectEnum.Login)
                    {
                        Login loginAux = databaseContext.Logins.Single(x => x.ServerId == schemaOfPrincipal.Database.Server.ServerId && x.LoginName == principal.Name);
                        this.SavePermissionForLogin(databaseContext, login, schemaOfPrincipal, schemaOfSecurable, command, permission, loginAux);
                    }
                    else
                    {
                        throw new Exception(string.Format("Invalid principal for the permission '{0}' for object type '{1}'.", command.Permission.Permission, permissionObjectType));
                    }
                }
                else if (principal.SecurableClass == SystemObjectEnum.DatabaseRole)
                {
                    if (permissionObjectType == SystemObjectEnum.Server || permissionObjectType == SystemObjectEnum.Endpoint || permissionObjectType == SystemObjectEnum.Login)
                    {
                        throw new Exception(string.Format("Invalid principal for the permission '{0}' for object type '{1}'.", command.Permission.Permission, permissionObjectType));
                    }
                    else
                    {
                        DatabaseRole databaseRole = databaseContext.DatabaseRoles.Single(x => x.ServerId == schemaOfPrincipal.Database.Server.ServerId && x.DatabaseId == schemaOfPrincipal.DatabaseId && x.DbRoleName == principal.Name);
                        this.SavePermissionForRole(databaseContext, login, schemaOfPrincipal, schemaOfSecurable, command, permission, databaseRole);
                    }
                }
            }

            return context;
        }

        /// <inheritdoc />
        public override void OnError(PipelineContext context)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Saves a new permission for the specified role.
        /// </summary>
        /// <param name="databaseContext">Pipeline context.</param>
        /// <param name="login">Login of the client</param>
        /// <param name="schemaOfPrincipal">Schema of the principal.</param>
        /// <param name="schemaOfSecurable">Schema of the securable.</param>
        /// <param name="command">Command to execute.</param>
        /// <param name="permission">Permission to assign.</param>
        /// <param name="principal">Principal to assign the permission.</param>
        protected abstract void SavePermissionForLogin(SpaceDbContext databaseContext, Login login, Schema schemaOfPrincipal, Schema schemaOfSecurable, PermissionsCommandNode command, PermissionNode permission, Login principal);

        /// <summary>
        /// Saves a new permission for the specified role.
        /// </summary>
        /// <param name="databaseContext">Pipeline context.</param>
        /// <param name="login">Login of the client</param>
        /// <param name="schemaOfPrincipal">Schema of the principal.</param>
        /// <param name="schemaOfSecurable">Schema of the securable.</param>
        /// <param name="command">Command to execute.</param>
        /// <param name="permission">Permission to assign.</param>
        /// <param name="user">Principal to assign the permission.</param>
        protected abstract void SavePermissionForUser(SpaceDbContext databaseContext, Login login, Schema schemaOfPrincipal, Schema schemaOfSecurable, PermissionsCommandNode command, PermissionNode permission, DatabaseUser user);

        /// <summary>
        /// Saves a new permission for the specified role.
        /// </summary>
        /// <param name="databaseContext">Pipeline context.</param>
        /// <param name="login">Login of the client</param>
        /// <param name="schemaOfPrincipal">Schema of the principal.</param>
        /// <param name="schemaOfSecurable">Schema of the securable.</param>
        /// <param name="command">Command to execute.</param>
        /// <param name="permission">Permission to assign.</param>
        /// <param name="role">Principal to assign the permission.</param>
        protected abstract void SavePermissionForRole(SpaceDbContext databaseContext, Login login, Schema schemaOfPrincipal, Schema schemaOfSecurable, PermissionsCommandNode command, PermissionNode permission, DatabaseRole role);
    }
}
