//-----------------------------------------------------------------------
// <copyright file="TakeOwnershipOfDatabaseFilter.cs" company="Integra.Space.Language">
//     Copyright (c) Integra.Space.Language. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using System.Linq;
    using Database;
    using Integra.Space.Language;

    /// <summary>
    /// Grant permission filter class.
    /// </summary>
    internal class TakeOwnershipOfDatabaseFilter : TakeOwnershipFilter
    {
        /// <inheritdoc />
        public override void OnError(PipelineContext context)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        protected override void TakeOwnership(TakeOwnershipCommandNode command, SpaceDbContext databaseContext, Login login, Schema schema)
        {
            Database database = databaseContext.Databases.Single(x => x.ServerId == schema.ServerId
                                            && x.DatabaseName == command.MainCommandObject.Name);

            // se le establece al usuario que esta ejecutando el comando como propietario del objeto especificado.
            database.OwnerServerId = login.ServerId;
            database.OwnerId = login.LoginId;

            databaseContext.SaveChanges();
        }
    }
}
