//-----------------------------------------------------------------------
// <copyright file="AddSecureObjectToRoleFilter.cs" company="Integra.Space.Language">
//     Copyright (c) Integra.Space.Language. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database;
    using Language;
    using Ninject;

    /// <summary>
    /// Grant permission filter class.
    /// </summary>
    internal class AddSecureObjectToRoleFilter : CommandFilter
    {
        /// <inheritdoc />
        public override PipelineContext Execute(PipelineContext context)
        {
            AddOrRemoveCommandNode command = (AddOrRemoveCommandNode)context.CommandContext.Command;
            HashSet<CommandObject> roles = command.Roles;
            HashSet<CommandObject> users = command.Users;

            SpaceDbContext databaseContext = context.Kernel.Get<SpaceDbContext>();
            DatabaseRole databaseRole = null;
            DatabaseUser databaseUser = null;
            foreach (CommandObject role in roles)
            {
                Schema schema = role.GetSchema(databaseContext, context.SecurityContext.Login);
                databaseRole = databaseContext.DatabaseRoles.Single(x => x.ServerId == schema.Database.Server.ServerId && x.DatabaseId == schema.DatabaseId && x.DbRoleName == role.Name);
                foreach (CommandObject user in users)
                {
                    databaseUser = databaseContext.DatabaseUsers.Single(x => x.ServerId == schema.Database.Server.ServerId && x.DatabaseId == schema.DatabaseId && x.DbUsrName == user.Name);
                    if (!databaseRole.DatabaseUsers.Any(x => x.ServerId == databaseUser.ServerId && x.DatabaseId == databaseUser.DatabaseId && x.DbUsrName == databaseUser.DbUsrName))
                    {
                        databaseRole.DatabaseUsers.Add(databaseUser);
                    }
                }
            }

            databaseContext.SaveChanges();

            return context;
        }

        /// <inheritdoc />
        public override void OnError(PipelineContext context)
        {
            throw new NotImplementedException();
        }
    }
}
