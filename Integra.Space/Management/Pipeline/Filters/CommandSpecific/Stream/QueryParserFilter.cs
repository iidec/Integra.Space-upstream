//-----------------------------------------------------------------------
// <copyright file="QueryParserFilter.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using Database;
    using Language;
    using Language.Runtime;
    using Ninject;

    /// <summary>
    /// Create command action class.
    /// </summary>
    internal class QueryParserFilter : CommandFilter
    {
        /// <inheritdoc />
        public override PipelineContext Execute(PipelineContext context)
        {
            CreateStreamNode command = (CreateStreamNode)context.CommandContext.Command;
            string streamName = command.MainCommandObject.Name;
            Schema schema = context.CommandContext.Schema;

            ManagementSchedulerFactory dsf = new ManagementSchedulerFactory();
            bool printLog = false;
            bool debugMode = false;
            bool measureElapsedTime = false;
            CompileContext compileContext = new CompileContext() { PrintLog = printLog, QueryName = streamName, Scheduler = dsf, DebugMode = debugMode, MeasureElapsedTime = measureElapsedTime, IsTestMode = true };
            
            SpaceAssemblyBuilder sasmBuilder = new SpaceAssemblyBuilder("SpaceQueryAssembly_" + compileContext.QueryName);
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

            string newAssemblyDirectoryPath = System.IO.Path.Combine(Environment.CurrentDirectory, "TempQueryAssemblies", schema.Database.Server.ServerName, schema.Database.DatabaseName, schema.SchemaName);
            System.IO.Directory.CreateDirectory(newAssemblyDirectoryPath);

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

            string oldAssemblyDirectoryPath = System.IO.Path.Combine(Environment.CurrentDirectory, assemblyFileName);
            System.IO.File.Move(oldAssemblyDirectoryPath, assemblyPath);

            // se recarga el assembly
            context.Assembly = Assembly.LoadFile(assemblyPath);

            return context;
        }
    }
}