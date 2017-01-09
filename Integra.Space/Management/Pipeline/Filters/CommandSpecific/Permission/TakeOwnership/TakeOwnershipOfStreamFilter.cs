//-----------------------------------------------------------------------
// <copyright file="TakeOwnershipOfStreamFilter.cs" company="Integra.Space.Language">
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
    internal class TakeOwnershipOfStreamFilter : TakeOwnershipFilter
    {
        /// <inheritdoc />
        public override void OnError(PipelineContext context)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        protected override void TakeOwnership(TakeOwnershipCommandNode command, SpaceDbContext databaseContext, Login login, Schema schema)
        {
            DatabaseUser user = command.MainCommandObject.GetUser(databaseContext, login);

            Stream stream = databaseContext.Streams.Single(x => x.ServerId == schema.ServerId
                                            && x.DatabaseId == schema.DatabaseId
                                            && x.SchemaId == schema.SchemaId
                                            && x.StreamName == command.MainCommandObject.Name);

            // se le establece al usuario que esta ejecutando el comando como propietario del objeto especificado.
            stream.OwnerServerId = user.ServerId;
            stream.OwnerDatabaseId = user.DatabaseId;
            stream.OwnerId = user.DbUsrId;

            databaseContext.SaveChanges();
        }
    }
}
