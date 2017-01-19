//-----------------------------------------------------------------------
// <copyright file="TakeOwnershipOfSchemaFilter.cs" company="Integra.Space.Language">
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
    internal class TakeOwnershipOfSchemaFilter : TakeOwnershipFilter
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

            Schema databaseSchema = databaseContext.Schemas.Single(x => x.ServerId == schema.ServerId
                                            && x.DatabaseId == schema.DatabaseId
                                            && x.SchemaName == command.MainCommandObject.Name);

            // se le establece al usuario que esta ejecutando el comando como propietario del objeto especificado.
            databaseSchema.OwnerServerId = user.ServerId;
            databaseSchema.OwnerDatabaseId = user.DatabaseId;
            databaseSchema.OwnerId = user.DbUsrId;

            databaseContext.SaveChanges();
        }
    }
}
