﻿//-----------------------------------------------------------------------
// <copyright file="AlterLoginFilter.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System.Collections.Generic;
    using System.Linq;
    using Database;

    /// <summary>
    /// Filter alter user class.
    /// </summary>
    internal class AlterLoginFilter : AlterEntityFilter<Language.AlterLoginNode, Common.LoginOptionEnum>
    {
        /// <inheritdoc />
        protected override void EditEntity(Language.AlterLoginNode command, Dictionary<Common.LoginOptionEnum, object> options, Schema schema, SpaceDbContext databaseContext)
        {
            Login login = databaseContext.Logins.Single(x => x.ServerId == schema.ServerId
                                            && x.LoginName == command.MainCommandObject.Name);

            if (options.ContainsKey(Common.LoginOptionEnum.Default_Database))
            {
                string defaultDatabaseName = options[Common.LoginOptionEnum.Default_Database].ToString();
                Database database = databaseContext.Databases.Single(x => x.ServerId == schema.ServerId && x.DatabaseName == defaultDatabaseName);
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

            if (command.Options.ContainsKey(Common.LoginOptionEnum.Status))
            {
                login.IsActive = (bool)command.Options[Common.LoginOptionEnum.Status];
            }

            databaseContext.SaveChanges();
        }
    }
}
