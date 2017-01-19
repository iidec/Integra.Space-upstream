//-----------------------------------------------------------------------
// <copyright file="ParseQueryForCreateStreamFilter.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System.Reflection.Emit;
    using Database;
    using Language;
    using Ninject;

    /// <summary>
    /// Create command action class.
    /// </summary>
    internal class ParseQueryForCreateStreamFilter : ParseQueryFilter
    {
        /// <inheritdoc />
        public override PipelineContext Execute(PipelineContext context)
        {
            CreateStreamNode command = (CreateStreamNode)context.CommandContext.Command;
            Schema schema = command.MainCommandObject.GetSchema(context.Kernel.Get<SpaceDbContext>(), context.SecurityContext.Login);
            string typeSignature = string.Format("{0}_{1}", context.AssemblyBuilder.GetName().Name, command.MainCommandObject.Name);
            this.CompileQuery(typeSignature, command.ExecutionPlan, schema, context.SecurityContext.Login, context.Kernel, context.AssemblyBuilder);
            return context;
        }
    }
}