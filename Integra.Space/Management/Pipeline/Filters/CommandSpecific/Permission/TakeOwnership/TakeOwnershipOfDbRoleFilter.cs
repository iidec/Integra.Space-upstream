//-----------------------------------------------------------------------
// <copyright file="TakeOwnershipOfDbRoleFilter.cs" company="Integra.Space.Language">
//     Copyright (c) Integra.Space.Language. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using System.Linq;
    using Database;
    using Integra.Space.Language;
    using Ninject;

    /// <summary>
    /// Grant permission filter class.
    /// </summary>
    internal class TakeOwnershipOfDbRoleFilter : CommandFilter
    {
        /// <inheritdoc />
        public override PipelineContext Execute(PipelineContext context)
        {
            Schema schema = context.CommandContext.Schema;
            TakeOwnershipCommandNode command = (TakeOwnershipCommandNode)context.CommandContext.Command;
            SpaceDbContext databaseContext = context.Kernel.Get<SpaceDbContext>();
            DatabaseUser user = context.SecurityContext.User;

            DatabaseRole role = databaseContext.DatabaseRoles.Single(x => x.ServerId == schema.ServerId
                                            && x.DatabaseId == schema.DatabaseId
                                            && x.DbRoleName == command.MainCommandObject.Name);

            // se le establece al usuario que esta ejecutando el comando como propietario del objeto especificado.
            role.OwnerServerId = user.ServerId;
            role.OwnerDatabaseId = user.DatabaseId;
            role.OwnerId = user.DbUsrId;

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
