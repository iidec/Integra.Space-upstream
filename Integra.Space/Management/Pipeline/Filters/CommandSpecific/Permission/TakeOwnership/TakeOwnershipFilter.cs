//-----------------------------------------------------------------------
// <copyright file="TakeOwnershipFilter.cs" company="Integra.Space.Language">
//     Copyright (c) Integra.Space.Language. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using Database;
    using Ninject;

    /// <summary>
    /// Grant permission filter class.
    /// </summary>
    internal abstract class TakeOwnershipFilter : CommandFilter
    {
        /// <inheritdoc />
        public override PipelineContext Execute(PipelineContext context)
        {
            Language.TakeOwnershipCommandNode command = (Language.TakeOwnershipCommandNode)context.CommandContext.Command;
            SpaceDbContext databaseContext = context.Kernel.Get<SpaceDbContext>();
            Schema schema = command.MainCommandObject.GetSchema(databaseContext, context.SecurityContext.Login);
            this.TakeOwnership(command, databaseContext, context.SecurityContext.Login, schema);
            return context;
        }

        /// <inheritdoc />
        public override void OnError(PipelineContext context)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Takes ownership of the specified object at the command.
        /// </summary>
        /// <param name="command">Executing command.</param>
        /// <param name="databaseContext">Database context.</param>
        /// <param name="login">Login of the client.</param>
        /// <param name="schema">Schema of the object of command.</param>
        protected abstract void TakeOwnership(Language.TakeOwnershipCommandNode command, SpaceDbContext databaseContext, Login login, Schema schema);
    }
}
