//-----------------------------------------------------------------------
// <copyright file="ParseQueryForAlterStreamFilter.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using Common;
    using Database;
    using Language;
    using Ninject;

    /// <summary>
    /// Create command action class.
    /// </summary>
    internal class ParseQueryForAlterStreamFilter : ParseQueryFilter
    {
        /// <inheritdoc />
        public override PipelineContext Execute(PipelineContext context)
        {
            AlterStreamNode command = (AlterStreamNode)context.CommandContext.Command;
            Schema schema = command.MainCommandObject.GetSchema(context.Kernel.Get<SpaceDbContext>(), context.SecurityContext.Login);
            if (command.Options.ContainsKey(StreamOptionEnum.Query))
            {
                this.CompileQuery(command.MainCommandObject.Name, command.ExecutionPlan, schema);
            }

            return context;
        }
    }
}