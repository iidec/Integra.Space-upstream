//-----------------------------------------------------------------------
// <copyright file="FilterQueryParser.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System.Reflection;
    using System.Reflection.Emit;
    using Language;
    using Language.Runtime;
    using Models;
    using Ninject;
    using Repos;

    /// <summary>
    /// Create command action class.
    /// </summary>
    internal class FilterQueryParser : CommandFilter
    {
        /// <inheritdoc />
        public override PipelineExecutionCommandContext Execute(PipelineExecutionCommandContext context)
        {
            IRepository<Stream> sr = context.Kernel.Get<IRepository<Stream>>();
            Stream stream = sr.FindByName(context.Command.ObjectName);
            if (stream == null)
            {
                throw new System.Exception("The stream does not exist.");
            }

            ManagementSchedulerFactory dsf = new ManagementSchedulerFactory();
            bool printLog = false;
            bool debugMode = false;
            bool measureElapsedTime = false;
            CompileContext compileContext = new CompileContext() { PrintLog = printLog, QueryName = context.Command.ObjectName, Scheduler = dsf, DebugMode = debugMode, MeasureElapsedTime = measureElapsedTime, IsTestMode = true };

            QueryParser parser = new QueryParser(((CreateAndAlterStreamNode)context.Command).Query);
                        
            PlanNode executionPlan = parser.Evaluate();

            SpaceAssemblyBuilder sasmBuilder = new SpaceAssemblyBuilder("SpaceQueryAssembly_" + compileContext.QueryName);
            AssemblyBuilder asmBuilder = sasmBuilder.CreateAssemblyBuilder();
            SpaceModuleBuilder modBuilder = new SpaceModuleBuilder(asmBuilder);
            modBuilder.CreateModuleBuilder();

            TreeTransformations tf = new TreeTransformations(asmBuilder, executionPlan);
            tf.Transform();

            compileContext.AsmBuilder = asmBuilder;
            CodeGenerator te = new CodeGenerator(compileContext);

            Assembly asm = te.Compile(executionPlan);
            stream.StreamAssembly = new StreamAssembly(asm);

            return context;
        }
    }
}