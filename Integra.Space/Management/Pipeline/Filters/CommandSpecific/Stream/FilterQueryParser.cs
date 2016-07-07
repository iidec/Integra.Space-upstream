//-----------------------------------------------------------------------
// <copyright file="FilterQueryParser.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System.Reflection;
    using System.Reflection.Emit;
    using Common.CommandContext;
    using Language;
    using Language.Runtime;
    
    /// <summary>
    /// Create command action class.
    /// </summary>
    internal abstract class FilterQueryParser : Filter<PipelineCommandContext, PipelineCommandContext>
    {
        /// <inheritdoc />
        public override PipelineCommandContext Execute(PipelineCommandContext input)
        {
            ManagementSchedulerFactory dsf = new ManagementSchedulerFactory();
            bool printLog = false;
            bool debugMode = false;
            bool measureElapsedTime = false;
            CompileContext context = new CompileContext() { PrintLog = printLog, QueryName = string.Empty, Scheduler = dsf, DebugMode = debugMode, MeasureElapsedTime = measureElapsedTime, IsTestMode = true };

            QueryParser parser = new QueryParser(((CreateAndAlterStreamNode)input.Command).Query);
                        
            PlanNode executionPlan = parser.Evaluate();

            SpaceAssemblyBuilder sasmBuilder = new SpaceAssemblyBuilder("SpaceQueryAssembly_" + context.QueryName);
            AssemblyBuilder asmBuilder = sasmBuilder.CreateAssemblyBuilder();
            SpaceModuleBuilder modBuilder = new SpaceModuleBuilder(asmBuilder);
            modBuilder.CreateModuleBuilder();

            TreeTransformations tf = new TreeTransformations(asmBuilder, executionPlan);
            tf.Transform();

            context.AsmBuilder = asmBuilder;
            CodeGenerator te = new CodeGenerator(context);

            Assembly asm = te.Compile(executionPlan);
            return input;
        }
    }
}