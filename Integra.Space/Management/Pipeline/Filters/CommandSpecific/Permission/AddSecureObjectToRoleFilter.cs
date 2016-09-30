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
            Schema schema = context.CommandContext.Schema;
            AddCommandNode command = (AddCommandNode)context.CommandContext.Command;
            HashSet<CommandObject> roles = command.Roles;
            HashSet<CommandObject> users = command.Users;

            SpaceDbContext databaseContext = context.Kernel.Get<SpaceDbContext>();
            DatabaseRole databaseRole = null;
            DatabaseUser databaseUser = null;
            foreach (CommandObject role in roles)
            {
                databaseRole = databaseContext.DatabaseRoles.Single(x => x.ServerId == schema.Database.Server.ServerId && x.DatabaseId == schema.DatabaseId && x.DbRoleName == role.Name);
                foreach (CommandObject user in users)
                {
                    databaseUser = databaseContext.DatabaseUsers.Single(x => x.ServerId == schema.Database.Server.ServerId && x.DatabaseId == schema.DatabaseId && x.DbUsrName == user.Name);
                    databaseRole.DatabaseUsers.Add(databaseUser);
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
