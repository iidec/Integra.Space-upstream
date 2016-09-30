//-----------------------------------------------------------------------
// <copyright file="AlterUserFilter.cs" company="Integra.Space">
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
    internal class AlterUserFilter : AlterEntityFilter
    {
        /// <inheritdoc />
        protected override void EditEntity(PipelineContext context)
        {
            Dictionary<UserOptionEnum, object> options = ((Language.CreateObjectNode<UserOptionEnum>)context.CommandContext.Command).Options;

            SpaceDbContext databaseContext = context.Kernel.Get<SpaceDbContext>();
            DatabaseUser user = databaseContext.DatabaseUsers.Single(x => x.ServerId == context.CommandContext.Schema.ServerId
                                            && x.DatabaseId == context.CommandContext.Schema.DatabaseId
                                            && x.DbUsrName == ((Language.DDLCommand)context.CommandContext.Command).MainCommandObject.Name);

            if (options.ContainsKey(Common.UserOptionEnum.Default_Schema))
            {
                Schema schema = databaseContext.Schemas.Single(x => x.ServerId == context.CommandContext.Schema.ServerId && x.DatabaseId == context.CommandContext.Schema.DatabaseId && x.SchemaName == options[Common.UserOptionEnum.Default_Schema].ToString());
                user.DefaultSchema = schema;
            }

            if (options.ContainsKey(Common.UserOptionEnum.Name))
            {
                user.DbUsrName = options[Common.UserOptionEnum.Name].ToString();
            }

            databaseContext.SaveChanges();
        }
    }
}
