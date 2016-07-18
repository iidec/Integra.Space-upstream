//-----------------------------------------------------------------------
// <copyright file="ValidatePermissions.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using Common;
    using Models;
    using Ninject;
    using Repos;

    /// <summary>
    /// Filter lock class.
    /// </summary>
    internal sealed class ValidatePermissions : CommandFilter
    {
        /// <inheritdoc />
        public override PipelineExecutionCommandContext Execute(PipelineExecutionCommandContext input)
        {
            string userIdentifier = input.User.Identifier;
            string spaceObjectIdentifier = input.Command.ObjectName;
            SpaceObjectEnum spaceObjectType = input.Command.SpaceObjectType;
            SpaceActionCommandEnum commandType = input.Command.Action;

            PermissionCacheRepository permissionRepo = (PermissionCacheRepository)input.Kernel.Get<IRepository<Permission>>();

            Permission permission = null;
            if (commandType == SpaceActionCommandEnum.Create)
            {
                permission = permissionRepo.GetPermission(input.User, spaceObjectType);
            }
            else
            {
                permission = permissionRepo.GetPermission(input.User, spaceObjectType, spaceObjectIdentifier);
            }

            if (permission == null)
            {
                throw new Exception("Invalid permissions.");
            }

            if (!((permission.Value & (int)input.Command.PermissionValue) == (int)input.Command.PermissionValue))
            {
                throw new Exception("Invalid permissions.");
            }

            return input;
        }

        /// <inheritdoc />
        public override void OnError(PipelineExecutionCommandContext e)
        {
        }
    }
}