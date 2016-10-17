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

    /// <summary>
    /// Filter create source class.
    /// </summary>
    internal class CreateRoleFilter : CreateEntityFilter<Language.CreateRoleNode, RoleOptionEnum>
    {
        /// <inheritdoc />
        protected override void CreateEntity(Language.CreateRoleNode command, Dictionary<RoleOptionEnum, object> options, Login login, DatabaseUser user, Schema schema, SpaceDbContext databaseContext)
        {
            DatabaseRole role = new DatabaseRole();
            role.ServerId = schema.ServerId;
            role.DatabaseId = schema.DatabaseId;
            role.DbRoleId = Guid.NewGuid();
            role.DbRoleName = command.MainCommandObject.Name;

            role.OwnerServerId = user.ServerId;
            role.OwnerDatabaseId = user.DatabaseId;
            role.OwnerId = user.DbUsrId;

            role.IsActive = true;
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
                    schemaOfTheUser = userToAdd.GetSchema(databaseContext, login);
                    databaseUser = databaseContext.DatabaseUsers.Single(x => x.ServerId == schemaOfTheUser.Database.Server.ServerId && x.DatabaseId == schemaOfTheUser.DatabaseId && x.DbUsrName == userToAdd.Name);
                    role.DatabaseUsers.Add(databaseUser);
                }
            }

            // almaceno la nueva entidad y guardo los cambios
            databaseContext.DatabaseRoles.Add(role);
            databaseContext.SaveChanges();
        }
    }
}
