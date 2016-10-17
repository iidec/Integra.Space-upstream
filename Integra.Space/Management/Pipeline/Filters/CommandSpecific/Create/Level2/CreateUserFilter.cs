//-----------------------------------------------------------------------
// <copyright file="CreateUserFilter.cs" company="Integra.Space">
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

    /// <summary>
    /// Filter create user class.
    /// </summary>
    internal class CreateUserFilter : CreateEntityFilter<CreateUserNode, UserOptionEnum>
    {
        /// <inheritdoc />
        protected override void CreateEntity(CreateUserNode command, Dictionary<UserOptionEnum, object> options, Login login, DatabaseUser user, Schema schema, SpaceDbContext databaseContext)
        {
            DatabaseUser newUser = new DatabaseUser();
            newUser.ServerId = schema.ServerId;
            newUser.DatabaseId = schema.DatabaseId;
            newUser.DbUsrId = Guid.NewGuid();
            newUser.DbUsrName = command.MainCommandObject.Name;
            newUser.IsActive = true;

            if (options.ContainsKey(UserOptionEnum.Default_Schema))
            {
                string schemaName = options[UserOptionEnum.Default_Schema].ToString();
                Schema defaultSchema = databaseContext.Schemas.Single(x => x.ServerId == schema.ServerId && x.DatabaseId == schema.DatabaseId && x.SchemaName == schemaName);

                newUser.DefaultSchemaServerId = defaultSchema.ServerId;
                newUser.DefaultSchemaDatabaseId = defaultSchema.DatabaseId;
                newUser.DefaultSchemaId = defaultSchema.SchemaId;
            }
            else
            {
                newUser.DefaultSchemaServerId = schema.ServerId;
                newUser.DefaultSchemaDatabaseId = schema.DatabaseId;
                newUser.DefaultSchemaId = schema.SchemaId;
            }

            Login loginForUser = login;
            if (options.ContainsKey(UserOptionEnum.Login))
            {
                string loginName = options[UserOptionEnum.Login].ToString();
                loginForUser = databaseContext.Logins.Single(x => x.ServerId == schema.ServerId && x.LoginName == loginName);
            }

            if (databaseContext.DatabaseUsers.Any(x => x.LoginServerId == loginForUser.ServerId && x.LoginId == loginForUser.LoginId && x.ServerId == newUser.ServerId && x.DatabaseId == newUser.DatabaseId))
            {
                throw new Exception(string.Format("The login '{0}' already have a user at the database '{1}'.", loginForUser.LoginName, newUser.Database.DatabaseName));
            }

            newUser.Login = loginForUser;

            newUser.IsActive = true;
            if (command.Options.ContainsKey(Common.UserOptionEnum.Status))
            {
                newUser.IsActive = (bool)command.Options[Common.UserOptionEnum.Status];
            }

            // almaceno la nueva entidad y guardo los cambios
            databaseContext.DatabaseUsers.Add(newUser);
            databaseContext.SaveChanges();
        }
    }
}
