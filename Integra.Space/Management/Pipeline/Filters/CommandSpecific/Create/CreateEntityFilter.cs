//-----------------------------------------------------------------------
// <copyright file="CreateEntityFilter.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System.Collections.Generic;
    using System.Linq;
    using Database;
    using Ninject;

    /// <summary>
    /// Filter create source class.
    /// </summary>
    /// <typeparam name="TCommand">Command type.</typeparam>
    /// <typeparam name="TOption">Command option type.</typeparam>
    internal abstract class CreateEntityFilter<TCommand, TOption> : CommandFilter where TCommand : Language.CreateObjectNode<TOption> where TOption : struct, System.IConvertible
    {
        /// <inheritdoc />
        public override PipelineContext Execute(PipelineContext context)
        {
            TCommand command = (TCommand)context.CommandContext.Command;
            Dictionary<TOption, object> options = command.Options;
            Schema schema = command.MainCommandObject.GetSchema(context.Kernel.Get<SpaceDbContext>(), context.SecurityContext.Login);
            SpaceDbContext databaseContext = context.Kernel.Get<SpaceDbContext>();
            Login login = context.SecurityContext.Login;
            Database database = command.MainCommandObject.GetDatabase(databaseContext, login);
            DatabaseUser user = login.DatabaseUsers.Where(x => x.DatabaseId == database.DatabaseId && x.ServerId == database.ServerId).SingleOrDefault();

            if (user != null)
            {
                this.CreateEntity(command, options, login, user, schema, databaseContext);
            }
            else
            {
                throw new System.Exception("You need to have a user mapped for the login '{0}' at the database '{1}' before you create an entity.");
            }

            return context;
        }

        /// <inheritdoc />
        public override void OnError(PipelineContext context)
        {
        }

        /// <summary>
        /// Creates a new entity.
        /// </summary>
        /// <param name="command">Command object.</param>
        /// <param name="options">Options of the command.</param>
        /// <param name="login">Login executing the command.</param>
        /// <param name="user">User executing the command.</param>
        /// <param name="schema">Schema of the command object.</param>
        /// <param name="databaseContext">Database context.</param>
        protected abstract void CreateEntity(TCommand command, Dictionary<TOption, object> options, Login login, DatabaseUser user, Schema schema, SpaceDbContext databaseContext);
    }
}
