//-----------------------------------------------------------------------
// <copyright file="PermissionOnStreamFilter.cs" company="Integra.Space.Language">
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
    internal class PermissionOnStreamFilter : PermissionFilter
    {
        /// <inheritdoc />
        protected override void SavePermissionForUser(CommandObject user, PipelineContext context, Schema schema, PermissionsCommandNode command, PermissionNode permission)
        {
            SpaceDbContext databaseContext = context.Kernel.Get<SpaceDbContext>();
            DatabaseUser databaseUser = databaseContext.DatabaseUsers.Single(x => x.ServerId == schema.Database.Server.ServerId && x.DatabaseId == schema.DatabaseId && x.DbUsrName == user.Name);

            StreamAssignedPermissionsToUser newPermission = new StreamAssignedPermissionsToUser();
            newPermission.SecurableClassId = databaseContext.SecurableClasses.Single(x => x.SecurableName.Equals(permission.ObjectType.ToString(), StringComparison.InvariantCultureIgnoreCase)).SecurableClassId;
            newPermission.GranularPermissionId = databaseContext.GranularPermissions.Single(x => x.GranularPermissionName.Replace(" ", string.Empty).Equals(permission.Permission.ToString(), StringComparison.InvariantCultureIgnoreCase)).GranularPermissionId;
            newPermission.WithGrantOption = command.PermissionOption;
            newPermission.DatabaseUser = databaseUser;
            newPermission.Stream = databaseContext.Streams.Single(x => x.ServerId == schema.ServerId && x.DatabaseId == schema.DatabaseId && x.SchemaId == schema.SchemaId && x.StreamName == permission.ObjectName);
            
            Func<StreamAssignedPermissionsToUser, bool> predicate = x => x.DbUsrServerId == newPermission.DatabaseUser.ServerId
                                                                             && x.DbUsrDatabaseId == newPermission.DatabaseUser.DatabaseId
                                                                             && x.DbUsrId == newPermission.DatabaseUser.DbUsrId
                                                                             && x.StreamServerId == newPermission.Stream.ServerId
                                                                             && x.StreamDatabaseId == newPermission.Stream.DatabaseId
                                                                             && x.StreamSchemaId == newPermission.Stream.SchemaId
                                                                             && x.StreamId == newPermission.Stream.StreamId
                                                                             && x.GranularPermissionId == newPermission.GranularPermissionId
                                                                             && x.SecurableClassId == newPermission.SecurableClassId;

            if (!databaseContext.StreamAssignedPermissionsToUsers.Any(predicate))
            {
                if (command.Action == Common.ActionCommandEnum.Grant)
                {
                    newPermission.Granted = true;
                    databaseContext.StreamAssignedPermissionsToUsers.Add(newPermission);
                }
                else if (command.Action == Common.ActionCommandEnum.Deny)
                {
                    newPermission.Denied = true;
                    databaseContext.StreamAssignedPermissionsToUsers.Add(newPermission);
                }
                else
                {
                    throw new Exception("Only grant, deny or revoke command allowed.");
                }                
            }
            else
            {
                StreamAssignedPermissionsToUser permissionToUpdate = databaseContext.StreamAssignedPermissionsToUsers.Single(predicate);
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
                    databaseContext.StreamAssignedPermissionsToUsers.Remove(permissionToUpdate);
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

            StreamAssignedPermissionsToDBRole newPermission = new StreamAssignedPermissionsToDBRole();
            newPermission.SecurableClassId = databaseContext.SecurableClasses.Single(x => x.SecurableName.Equals(permission.ObjectType.ToString(), StringComparison.InvariantCultureIgnoreCase)).SecurableClassId;
            newPermission.GranularPermissionId = databaseContext.GranularPermissions.Single(x => x.GranularPermissionName.Replace(" ", string.Empty).Equals(permission.Permission.ToString(), StringComparison.InvariantCultureIgnoreCase)).GranularPermissionId;
            newPermission.WithGrantOption = command.PermissionOption;
            newPermission.DatabaseRole = databaseRole;
            newPermission.Stream = databaseContext.Streams.Single(x => x.ServerId == schema.ServerId && x.DatabaseId == schema.DatabaseId && x.SchemaId == schema.SchemaId && x.StreamName == permission.ObjectName);

            Func<StreamAssignedPermissionsToDBRole, bool> predicate = x => x.DbRoleServerId == newPermission.DatabaseRole.ServerId
                                                                             && x.DbRoleDatabaseId == newPermission.DatabaseRole.DatabaseId
                                                                             && x.DbRoleId == newPermission.DatabaseRole.DbRoleId
                                                                             && x.StreamServerId == newPermission.Stream.ServerId
                                                                             && x.StreamDatabaseId == newPermission.Stream.DatabaseId
                                                                             && x.StreamSchemaId == newPermission.Stream.SchemaId
                                                                             && x.StreamId == newPermission.Stream.StreamId
                                                                             && x.GranularPermissionId == newPermission.GranularPermissionId
                                                                             && x.SecurableClassId == newPermission.SecurableClassId;

            if (!databaseContext.StreamAssignedPermissionsToDBRoles.Any(predicate))
            {
                if (command.Action == Common.ActionCommandEnum.Grant)
                {
                    newPermission.Granted = true;
                    databaseContext.StreamAssignedPermissionsToDBRoles.Add(newPermission);
                }
                else if (command.Action == Common.ActionCommandEnum.Deny)
                {
                    newPermission.Denied = true;
                    databaseContext.StreamAssignedPermissionsToDBRoles.Add(newPermission);
                }
                else
                {
                    throw new Exception("Only grant, deny or revoke command allowed.");
                }
            }
            else
            {
                StreamAssignedPermissionsToDBRole permissionToUpdate = databaseContext.StreamAssignedPermissionsToDBRoles.Single(predicate);
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
                    databaseContext.StreamAssignedPermissionsToDBRoles.Remove(permissionToUpdate);
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
