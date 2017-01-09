//-----------------------------------------------------------------------
// <copyright file="AlterRoleFilter.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System.Collections.Generic;
    using System.Linq;
    using Common;
    using Database;
    using Language;
    using Ninject;

    /// <summary>
    /// Filter alter user class.
    /// </summary>
    internal class AlterRoleFilter : AlterEntityFilter<AlterRoleNode, RoleOptionEnum>
    {
        /// <summary>
        /// Login of the client.
        /// </summary>
        private Login login;

        /// <inheritdoc />
        public override PipelineContext Execute(PipelineContext context)
        {
            AlterRoleNode command = (AlterRoleNode)context.CommandContext.Command;
            Dictionary<RoleOptionEnum, object> options = command.Options;
            this.login = context.SecurityContext.Login;
            Schema schema = command.MainCommandObject.GetSchema(context.Kernel.Get<SpaceDbContext>(), this.login);
            SpaceDbContext databaseContext = context.Kernel.Get<SpaceDbContext>();

            this.EditEntity(command, options, schema, databaseContext);
            return context;
        }

        /// <inheritdoc />
        protected override void EditEntity(AlterRoleNode command, Dictionary<RoleOptionEnum, object> options, Schema schema, SpaceDbContext databaseContext)
        {
            DatabaseRole role = databaseContext.DatabaseRoles.Single(x => x.ServerId == schema.ServerId
                                            && x.DatabaseId == schema.DatabaseId
                                            && x.DbRoleName == command.MainCommandObject.Name);
            
            if (options.ContainsKey(Common.RoleOptionEnum.Name))
            {
                role.DbRoleName = options[Common.RoleOptionEnum.Name].ToString();
            }

            if (command.Options.ContainsKey(Common.RoleOptionEnum.Status))
            {
                role.IsActive = (bool)command.Options[Common.RoleOptionEnum.Status];
            }

            if (command.Options.ContainsKey(RoleOptionEnum.Add))
            {
                HashSet<Language.CommandObject> users = (HashSet<Language.CommandObject>)command.Options[RoleOptionEnum.Add];
                DatabaseUser databaseUser = null;
                Schema schemaOfTheUser = null;
                foreach (Language.CommandObject userToAdd in users)
                {
                    schemaOfTheUser = userToAdd.GetSchema(databaseContext, this.login);
                    databaseUser = databaseContext.DatabaseUsers.Single(x => x.ServerId == schemaOfTheUser.Database.Server.ServerId && x.DatabaseId == schemaOfTheUser.DatabaseId && x.DbUsrName == userToAdd.Name);
                    if (!role.DatabaseUsers.Any(x => x.ServerId == databaseUser.ServerId && x.DatabaseId == databaseUser.DatabaseId && x.DbUsrName == databaseUser.DbUsrName))
                    {
                        role.DatabaseUsers.Add(databaseUser);
                    }
                }
            }

            if (command.Options.ContainsKey(RoleOptionEnum.Remove))
            {
                HashSet<Language.CommandObject> users = (HashSet<Language.CommandObject>)command.Options[RoleOptionEnum.Remove];
                DatabaseUser databaseUser = null;
                Schema schemaOfTheUser = null;
                foreach (Language.CommandObject userToRemove in users)
                {
                    schemaOfTheUser = userToRemove.GetSchema(databaseContext, this.login);
                    databaseUser = databaseContext.DatabaseUsers.Single(x => x.ServerId == schemaOfTheUser.Database.Server.ServerId && x.DatabaseId == schemaOfTheUser.DatabaseId && x.DbUsrName == userToRemove.Name);
                    if (role.DatabaseUsers.Any(x => x.ServerId == databaseUser.ServerId && x.DatabaseId == databaseUser.DatabaseId && x.DbUsrName == databaseUser.DbUsrName))
                    {
                        role.DatabaseUsers.Remove(databaseUser);
                    }
                }
            }

            databaseContext.SaveChanges();
        }
    }
}
