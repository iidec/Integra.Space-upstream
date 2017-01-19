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
    internal class AlterUserFilter : AlterEntityFilter<AlterUserNode, UserOptionEnum>
    {
        /// <inheritdoc />
        protected override void EditEntity(AlterUserNode command, Dictionary<UserOptionEnum, object> options, Login login, Schema schema, SpaceDbContext databaseContext)
        {
            DatabaseUser user = databaseContext.DatabaseUsers.Single(x => x.ServerId == schema.ServerId
                                            && x.DatabaseId == schema.DatabaseId
                                            && x.DbUsrName == command.MainCommandObject.Name);

            if (options.ContainsKey(Common.UserOptionEnum.Default_Schema))
            {
                string schemaName = options[Common.UserOptionEnum.Default_Schema].ToString();
                Schema defaultSchema = databaseContext.Schemas.Single(x => x.ServerId == schema.ServerId && x.DatabaseId == schema.DatabaseId && x.SchemaName == schemaName);
                user.DefaultSchema = defaultSchema;
            }

            if (options.ContainsKey(Common.UserOptionEnum.Name))
            {
                user.DbUsrName = options[Common.UserOptionEnum.Name].ToString();
            }

            if (options.ContainsKey(UserOptionEnum.Login))
            {
                string loginName = options[UserOptionEnum.Login].ToString();
                Login loginForUser = databaseContext.Logins.Single(x => x.ServerId == schema.ServerId && x.LoginName == loginName);

                if (databaseContext.DatabaseUsers.Any(x => x.LoginServerId == loginForUser.ServerId && x.LoginId == loginForUser.LoginId && x.ServerId == user.ServerId && x.DatabaseId == user.DatabaseId))
                {
                    throw new Exception(string.Format("The login '{0}' already exist at the server '{1}'.", loginForUser.LoginName, loginForUser.Server.ServerName));
                }

                user.Login = loginForUser;
            }

            if (command.Options.ContainsKey(Common.UserOptionEnum.Status))
            {
                user.IsActive = (bool)command.Options[Common.UserOptionEnum.Status];
            }

            databaseContext.SaveChanges();
        }
    }
}
