//-----------------------------------------------------------------------
// <copyright file="DropEntityFilter.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using Database;
    using Integra.Space.Pipeline;
    using Ninject;

    /// <summary>
    /// Drop entity class.
    /// </summary>
    internal abstract class DropEntityFilter : CommandFilter
    {
        /// <inheritdoc />
        public override PipelineContext Execute(PipelineContext context)
        {
            Language.DropObjectNode command = (Language.DropObjectNode)context.CommandContext.Command;
            SpaceDbContext databaseContext = context.Kernel.Get<SpaceDbContext>();
            Schema schema = command.MainCommandObject.GetSchema(context.Kernel.Get<SpaceDbContext>(), context.SecurityContext.Login);
            this.DropEntity(databaseContext, schema, command.MainCommandObject.Name);
            return context;
        }

        /// <inheritdoc />
        public override void OnError(PipelineContext context)
        {
        }

        /// <summary>
        /// Removes the specified entity.
        /// </summary>
        /// <param name="databaseContext">Database context.</param>
        /// <param name="schema">Schema of the object.</param>
        /// <param name="name">Object name.</param>
        protected virtual void DropEntity(SpaceDbContext databaseContext, Schema schema, string name)
        {
        }
    }
}
