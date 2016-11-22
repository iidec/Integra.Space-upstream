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
    using Database;
    using Language;
    using Language.Runtime;
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

            /*
            ManagementSchedulerFactory dsf = new ManagementSchedulerFactory();
            bool printLog = false;
            bool debugMode = false;
            bool measureElapsedTime = false;
            bool isTestMode = false;
            CompileContext compileContext = new CompileContext() { PrintLog = printLog, QueryName = streamName, Scheduler = dsf, DebugMode = debugMode, MeasureElapsedTime = measureElapsedTime, IsTestMode = isTestMode };
            
            SpaceAssemblyBuilder sasmBuilder = new SpaceAssemblyBuilder(string.Concat("SpaceQueryAssembly_", streamName));
            AssemblyBuilder asmBuilder = sasmBuilder.CreateAssemblyBuilder();
            SpaceModuleBuilder modBuilder = new SpaceModuleBuilder(asmBuilder);
            modBuilder.CreateModuleBuilder();

            TreeTransformations tf = new TreeTransformations(asmBuilder, command.ExecutionPlan);
            tf.Transform();

            compileContext.AsmBuilder = asmBuilder;
            CodeGenerator te = new CodeGenerator(compileContext);
            Assembly asm = te.Compile(command.ExecutionPlan);
            
            string assemblyFileName = asmBuilder.GetName().Name + SpaceAssemblyBuilder.FILEEXTENSION;
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
            */

            return context;
        }

        /// <summary>
        /// Compile a query of a stream.
        /// </summary>
        /// <param name="streamName">Stream name.</param>
        /// <param name="executionPlan">Execution plan of the query.</param>
        /// <param name="schema">Specified schema of the stream.</param>
        protected void CompileQuery(string streamName, PlanNode executionPlan, Schema schema)
        {
            ManagementSchedulerFactory dsf = new ManagementSchedulerFactory();
            bool printLog = false;
            bool debugMode = false;
            bool measureElapsedTime = false;
            bool isTestMode = false;
            CompilerConfiguration compileContext = new CompilerConfiguration() { PrintLog = printLog, QueryName = streamName, Scheduler = dsf, DebugMode = debugMode, MeasureElapsedTime = measureElapsedTime, IsTestMode = isTestMode };

            SpaceAssemblyBuilder sasmBuilder = new SpaceAssemblyBuilder(streamName);
            AssemblyBuilder asmBuilder = sasmBuilder.CreateAssemblyBuilder();
            SpaceModuleBuilder modBuilder = new SpaceModuleBuilder(asmBuilder);
            modBuilder.CreateModuleBuilder();

            TreeTransformations tf = new TreeTransformations(asmBuilder, executionPlan);
            tf.Transform();

            compileContext.AsmBuilder = asmBuilder;
            CodeGenerator te = new CodeGenerator(compileContext);
            Assembly asm = te.Compile(executionPlan);

            string assemblyFileName = asmBuilder.GetName().Name + SpaceAssemblyBuilder.FILEEXTENSION;
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