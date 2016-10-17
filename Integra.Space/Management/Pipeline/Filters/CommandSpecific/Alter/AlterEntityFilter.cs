//-----------------------------------------------------------------------
// <copyright file="AlterEntityFilter.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System.Collections.Generic;
    using Database;
    using Ninject;

    /// <summary>
    /// Filter create source class.
    /// </summary>
    /// <typeparam name="TCommand">Command type.</typeparam>
    /// <typeparam name="TOption">Command option type.</typeparam>
    internal abstract class AlterEntityFilter<TCommand, TOption> : CommandFilter where TCommand : Language.AlterObjectNode<TOption> where TOption : struct, System.IConvertible
    {
        /// <inheritdoc />
        public override PipelineContext Execute(PipelineContext context)
        {
            TCommand command = (TCommand)context.CommandContext.Command;
            Dictionary<TOption, object> options = command.Options;
            Schema schema = command.MainCommandObject.GetSchema(context.Kernel.Get<SpaceDbContext>(), context.SecurityContext.Login);
            SpaceDbContext databaseContext = context.Kernel.Get<SpaceDbContext>();

            this.EditEntity(command, options, schema, databaseContext);
            return context;
        }

        /// <inheritdoc />
        public override void OnError(PipelineContext context)
        {
        }

        /// <summary>
        /// Edits an entity.
        /// </summary>
        /// <param name="command">Command object.</param>
        /// <param name="options">Options of the command.</param>
        /// <param name="schema">Schema of the command object.</param>
        /// <param name="databaseContext">Database context.</param>
        protected abstract void EditEntity(TCommand command, Dictionary<TOption, object> options, Schema schema, SpaceDbContext databaseContext);
    }
}
