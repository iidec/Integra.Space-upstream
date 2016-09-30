//-----------------------------------------------------------------------
// <copyright file="AlterLoginFilter.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database;
    using Ninject;

    /// <summary>
    /// Filter alter user class.
    /// </summary>
    internal class AlterLoginFilter : AlterEntityFilter
    {
        /// <inheritdoc />
        protected override void EditEntity(PipelineContext context)
        {
            Dictionary<Common.LoginOptionEnum, object> options = ((Language.CreateObjectNode<Common.LoginOptionEnum>)context.CommandContext.Command).Options;

            SpaceDbContext databaseContext = context.Kernel.Get<SpaceDbContext>();
            Login login = databaseContext.Logins.Single(x => x.ServerId == context.CommandContext.Schema.ServerId
                                            && x.LoginName == ((Language.DDLCommand)context.CommandContext.Command).MainCommandObject.Name);

            if (options.ContainsKey(Common.LoginOptionEnum.Default_Database))
            {
                Database database = databaseContext.Databases.Single(x => x.ServerId == context.CommandContext.Schema.ServerId && x.DatabaseName == options[Common.LoginOptionEnum.Default_Database].ToString());
                login.Database = database;
            }

            if (options.ContainsKey(Common.LoginOptionEnum.Name))
            {
                login.LoginName = options[Common.LoginOptionEnum.Name].ToString();
            }

            if (options.ContainsKey(Common.LoginOptionEnum.Password))
            {
                login.LoginPassword = options[Common.LoginOptionEnum.Password].ToString();
            }

            databaseContext.SaveChanges();
        }
    }
}
