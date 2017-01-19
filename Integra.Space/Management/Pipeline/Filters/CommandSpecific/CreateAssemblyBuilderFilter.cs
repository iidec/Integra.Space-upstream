//-----------------------------------------------------------------------
// <copyright file="CreateAssemblyBuilderFilter.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System.Linq;
    using System.Reflection.Emit;
    using Common;
    using Compiler;
    using Database;
    using Language;
    using Ninject;

    /// <summary>
    /// Create command action class.
    /// </summary>
    internal class CreateAssemblyBuilderFilter : ParseQueryFilter
    {
        /// <inheritdoc />
        public override PipelineContext Execute(PipelineContext context)
        {
            SystemCommand command = context.CommandContext.Command;
            CommandObject commandObject = command.MainCommandObject;
            Login login = context.SecurityContext.Login;
            SpaceDbContext databaseContext = context.Kernel.Get<SpaceDbContext>();
            Schema schema = null;

            if (commandObject != null)
            {
                schema = commandObject.GetSchema(databaseContext, login);
            }
            else
            {
                if (command.Schema != null)
                {
                    schema = command.Schema.GetSchema(databaseContext, login);
                }
                else
                {
                    string databaseName = command.Database != null ? command.Database.Name : login.Database.DatabaseName;
                    schema = databaseContext.DatabaseUsers.Single(x => x.LoginId == login.LoginId && x.Database.DatabaseName == databaseName).DefaultSchema;
                }
            }

            string systemObjectName = string.Empty;
            if (command.MainCommandObject != null)
            {
                systemObjectName = command.MainCommandObject.Name;
            }
            else
            {
                systemObjectName = System.Guid.NewGuid().ToString();
            }

            string assemblySignature = string.Format("{0}_{1}_{2}_{3}", schema.Database.Server.ServerName.Replace(' ', '\0'), schema.Database.DatabaseName.Replace(' ', '\0'), schema.SchemaName.Replace(' ', '\0'), systemObjectName);

            SpaceAssemblyBuilder spaceAsmBuilder = new SpaceAssemblyBuilder(assemblySignature);
            AssemblyBuilder asmBuilder = spaceAsmBuilder.CreateAssemblyBuilder();
            SpaceModuleBuilder spaceModBuilder = new SpaceModuleBuilder(asmBuilder);
            spaceModBuilder.CreateModuleBuilder();

            // se agrega el assembly builder a DI
            context.AssemblyBuilder = asmBuilder;

            return context;
        }
    }
}