//-----------------------------------------------------------------------
// <copyright file="SpecificFiltersAggregator.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using Common;
    using Common.CommandContext;
    using Language;

    /// <summary>
    /// Specific filters aggregator class.
    /// </summary>
    internal class SpecificFiltersAggregator : Filter<SpaceCommand, ExecutionPipelineNode<PipelineCommandContext, PipelineCommandContext>>
    {
        /// <inheritdoc />
        public override ExecutionPipelineNode<PipelineCommandContext, PipelineCommandContext> Execute(SpaceCommand input)
        {
            if ((SpaceActionCommandEnum.CrudCommands & input.Action) == SpaceActionCommandEnum.Create)
            {
                return this.AddCrudSpecificActions(input);
            }
            else if ((SpaceActionCommandEnum.CrudCommands & input.Action) == SpaceActionCommandEnum.Create)
            {
                return this.AddStatusSpecificActions(input);
            }
            else if ((SpaceActionCommandEnum.CrudCommands & input.Action) == SpaceActionCommandEnum.Create)
            {
                return this.AddPermissionsSpecificActions(input);
            }
            else
            {
                throw new System.Exception("Not implement command. Command: " + input.Action);
            }
        }

        /// <summary>
        /// Gets the specific filters for the specified crud command.
        /// </summary>
        /// <param name="command">Context of the pipeline.</param>
        /// <returns>The specific pipeline.</returns>
        public ExecutionPipelineNode<PipelineCommandContext, PipelineCommandContext> AddCrudSpecificActions(SpaceCommand command)
        {
            switch (command.Action)
            {
                case SpaceActionCommandEnum.Create:
                    return new ExecutionPipelineNode<PipelineCommandContext, PipelineCommandContext>(new FilterCreateSource(), command);
                case SpaceActionCommandEnum.Alter:
                case SpaceActionCommandEnum.Drop:
                default:
                    throw new System.Exception("Not implemented crud command. Command: " + command.Action);
            }
        }

        /// <summary>
        /// Gets the specific filters for the specified status command.
        /// </summary>
        /// <param name="command">Context of the pipeline.</param>
        /// <returns>The specific pipeline.</returns>
        public ExecutionPipelineNode<PipelineCommandContext, PipelineCommandContext> AddStatusSpecificActions(SpaceCommand command)
        {
            switch (command.Action)
            {
                case SpaceActionCommandEnum.Start:
                case SpaceActionCommandEnum.Stop:
                default:
                    throw new System.Exception("Not implemented status command. Command: " + command.Action);
            }
        }

        /// <summary>
        /// Gets the specific filters for the specified permission command.
        /// </summary>
        /// <param name="command">Context of the pipeline.</param>
        /// <returns>The specific pipeline.</returns>
        public ExecutionPipelineNode<PipelineCommandContext, PipelineCommandContext> AddPermissionsSpecificActions(SpaceCommand command)
        {
            switch (command.Action)
            {
                case SpaceActionCommandEnum.Grant:
                case SpaceActionCommandEnum.Revoke:
                case SpaceActionCommandEnum.Deny:
                default:
                    throw new System.Exception("Not implemented permission command. Command: " + command.Action);
            }
        }
    }
}
