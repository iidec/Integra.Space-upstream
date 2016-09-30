//-----------------------------------------------------------------------
// <copyright file="AlterRoleFilter.cs" company="Integra.Space">
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
    using Language;
    using Ninject;

    /// <summary>
    /// Filter alter user class.
    /// </summary>
    internal class AlterRoleFilter : AlterEntityFilter
    {
        /// <inheritdoc />
        protected override void EditEntity(PipelineContext context)
        {
            Dictionary<RoleOptionEnum, object> options = ((Language.CreateObjectNode<RoleOptionEnum>)context.CommandContext.Command).Options;

            SpaceDbContext databaseContext = context.Kernel.Get<SpaceDbContext>();
            DatabaseRole role = databaseContext.DatabaseRoles.Single(x => x.ServerId == context.CommandContext.Schema.ServerId
                                            && x.DatabaseId == context.CommandContext.Schema.DatabaseId
                                            && x.DbRoleName == ((Language.DDLCommand)context.CommandContext.Command).MainCommandObject.Name);
            
            if (options.ContainsKey(Common.RoleOptionEnum.Name))
            {
                role.DbRoleName = options[Common.RoleOptionEnum.Name].ToString();
            }

            databaseContext.SaveChanges();
        }
    }
}
