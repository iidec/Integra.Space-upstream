//-----------------------------------------------------------------------
// <copyright file="CreateRoleFilter.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Common;
    using Database;
    using Ninject;

    /// <summary>
    /// Filter create source class.
    /// </summary>
    internal class CreateRoleFilter : CreateEntityFilter
    {
        /// <inheritdoc />
        protected override void CreateEntity(PipelineContext context)
        {
            Dictionary<RoleOptionEnum, object> options = ((Language.CreateRoleNode)context.CommandContext.Command).Options;
            Schema schema = context.CommandContext.Schema;

            DatabaseRole role = new DatabaseRole();
            role.ServerId = context.CommandContext.Schema.ServerId;
            role.DatabaseId = context.CommandContext.Schema.DatabaseId;
            role.DbRoleId = Guid.NewGuid();
            role.DbRoleName = ((Language.DDLCommand)context.CommandContext.Command).MainCommandObject.Name;

            DatabaseUser user = context.SecurityContext.User;
            role.OwnerServerId = user.ServerId;
            role.OwnerDatabaseId = user.DatabaseId;
            role.OwnerId = user.DbUsrId;

            // almaceno la nueva entidad y guardo los cambios
            SpaceDbContext databaseContext = context.Kernel.Get<SpaceDbContext>();
            databaseContext.DatabaseRoles.Add(role);
            databaseContext.SaveChanges();
        }
    }
}
