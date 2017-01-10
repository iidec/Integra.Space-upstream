//-----------------------------------------------------------------------
// <copyright file="TransformExecutionPlanFilter.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System.Reflection.Emit;
    using Language;
    using Ninject;

    /// <summary>
    /// Create command action class.
    /// </summary>
    internal class TransformExecutionPlanFilter : CommandFilter
    {
        /// <inheritdoc />
        public override PipelineContext Execute(PipelineContext context)
        {
            PlanNode executionPlan = null;
            SystemCommand command = context.CommandContext.Command;
            if (command is CreateStreamNode)
            {
                executionPlan = ((CreateStreamNode)command).ExecutionPlan;
            }
            else if (command is AlterStreamNode)
            {
                executionPlan = ((AlterStreamNode)command).ExecutionPlan;
            }
            else if (command is TemporalStreamNode)
            {
                executionPlan = ((TemporalStreamNode)command).ExecutionPlan;
            }
            else if (command is QueryCommandForMetadataNode)
            {
                executionPlan = ((QueryCommandForMetadataNode)command).ExecutionPlan;
            }

            Compiler.TreeTransformations tf = new Compiler.TreeTransformations(context.AssemblyBuilder, executionPlan, context.Kernel.Get<Compiler.ISourceTypeFactory>());
            tf.Transform();

            return context;
        }
    }
}