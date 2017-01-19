//-----------------------------------------------------------------------
// <copyright file="ParseQueryFilter.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;
    using Compiler;
    using Database;
    using Language;
    using Ninject;

    /// <summary>
    /// Create command action class.
    /// </summary>
    internal abstract class ParseQueryFilter : CommandFilter
    {
        /// <inheritdoc />
        public override PipelineContext Execute(PipelineContext context)
        {
            CreateStreamNode command = (CreateStreamNode)context.CommandContext.Command;            
            string streamName = command.MainCommandObject.Name;
            Schema schema = command.MainCommandObject.GetSchema(context.Kernel.Get<SpaceDbContext>(), context.SecurityContext.Login);            
            return context;
        }

        /// <summary>
        /// Compile a query of a stream.
        /// </summary>
        /// <param name="streamName">Stream name.</param>
        /// <param name="executionPlan">Execution plan of the query.</param>
        /// <param name="schema">Specified schema of the stream.</param>
        /// <param name="login">Login of the client.</param>
        /// <param name="kernel">DI kernel.</param>
        /// <param name="asmBuilder">Assembly builder.</param>
        protected void CompileQuery(string streamName, PlanNode executionPlan, Schema schema, Login login, IKernel kernel, AssemblyBuilder asmBuilder)
        {
            ManagementSchedulerFactory dsf = new ManagementSchedulerFactory();
            bool printLog = false;
            bool debugMode = false;
            bool measureElapsedTime = false;
            bool isTestMode = false;

            CodeGeneratorConfiguration config = new CodeGeneratorConfiguration(
                login,
                dsf,
                asmBuilder,
                kernel,
                printLog: printLog,
                debugMode: debugMode,
                measureElapsedTime: measureElapsedTime,
                isTestMode: isTestMode,
                queryName: streamName);
                                    
            CodeGenerator te = new CodeGenerator(config);
            Assembly asm = te.Compile(executionPlan);

            string assemblyFileName = asmBuilder.GetName().Name + "." + SpaceAssemblyBuilder.FILEEXTENSION;
            asmBuilder.Save(assemblyFileName); // , PortableExecutableKinds.PE32Plus, ImageFileMachine.IA64);

            // se crea el directorio donde se guardarán temporalmente los assemblies.
            string newAssemblyDirectoryPath = System.IO.Path.Combine(Environment.CurrentDirectory, "TempQueryAssemblies", schema.Database.Server.ServerName, schema.Database.DatabaseName, schema.SchemaName);
            System.IO.Directory.CreateDirectory(newAssemblyDirectoryPath);

            // se elimina el assembly si ya existe.
            string assemblyPath = System.IO.Path.Combine(newAssemblyDirectoryPath, assemblyFileName);
            if (System.IO.File.Exists(assemblyPath))
            {
                try
                {
                    System.IO.File.Delete(assemblyPath);
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Cannot delete the assembly '{0}'.", assemblyFileName), ex);
                }
            }

            // se mueve el assembly de su ubicación por defecto al directorio creado anteriormente.
            string oldAssemblyDirectoryPath = System.IO.Path.Combine(Environment.CurrentDirectory, assemblyFileName);
            System.IO.File.Move(oldAssemblyDirectoryPath, assemblyPath);
        }
    }
}