//-----------------------------------------------------------------------
// <copyright file="SecureContextBuilderFilter.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using System.Linq;
    using Common;
    using Database;
    using Integra.Space.Pipeline;
    using Language;
    using Ninject;

    /// <summary>
    /// Filter command parser class.
    /// </summary>
    internal class SecureContextBuilderFilter : CommandFilter
    {
        /// <inheritdoc />
        public override PipelineContext Execute(PipelineContext context)
        {
            /*
            SystemCommand command = context.CommandContext.Command;

            SpaceDbContext databaseContext = context.Kernel.Get<SpaceDbContext>();

            // obtener esquema
            Database db = context.CommandContext.Schema.Database;

            // obtengo el usuario que quiere ejecutar el comando. El usuario tambien se calcula en la clase CommandContext.            
            DatabaseUser user = context.SecurityContext.Login.DatabaseUsers.Where(x => x.DatabaseId == db.DatabaseId && x.ServerId == db.ServerId).Single();
            context.SecurityContext.User = user;
            */

            return context;
        }

        /// <inheritdoc />
        public override void OnError(PipelineContext e)
        {
            throw new NotImplementedException();
        }
    }
}
