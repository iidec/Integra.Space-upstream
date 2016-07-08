//-----------------------------------------------------------------------
// <copyright file="SpecificFiltersAggregator.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using System.Collections.Generic;
    using Common;

    /// <summary>
    /// Specific filters aggregator class.
    /// </summary>
    internal class SpecificFiltersAggregator : FirstPipelineFilter
    {
        /// <inheritdoc />
        public override PipelineContext Execute(PipelineContext context)
        {
            context.Pipeline = SpecificFilterSelector.GetSpecificFilter(new SpecificFilterKey(context.Command.Action, context.Command.SpaceObjectType));
            
            return context;
        }

        /// <summary>
        /// Add specific actions the execution pipeline.
        /// </summary>
        /// <param name="context">Context of the actual pipeline.</param>
        public void AddSpecificAtions(PipelineContext context)
        {
            if ((SpaceActionCommandEnum.CrudCommands & context.Command.Action) == SpaceActionCommandEnum.Create
                 || (SpaceActionCommandEnum.CrudCommands & context.Command.Action) == SpaceActionCommandEnum.Alter
                 || (SpaceActionCommandEnum.CrudCommands & context.Command.Action) == SpaceActionCommandEnum.Drop)
            {
                this.AddCrudSpecificActions(context);
            }
            else if ((SpaceActionCommandEnum.StatusCommands & context.Command.Action) == SpaceActionCommandEnum.Start
                || (SpaceActionCommandEnum.StatusCommands & context.Command.Action) == SpaceActionCommandEnum.Stop)
            {
                this.AddStatusSpecificActions(context);
            }
            else if ((SpaceActionCommandEnum.CommandsPermissions & context.Command.Action) == SpaceActionCommandEnum.Grant
                || (SpaceActionCommandEnum.CommandsPermissions & context.Command.Action) == SpaceActionCommandEnum.Deny
                || (SpaceActionCommandEnum.CommandsPermissions & context.Command.Action) == SpaceActionCommandEnum.Revoke)
            {
                this.AddPermissionsSpecificActions(context);
            }
            else
            {
                throw new System.Exception("Not implement command. Command: " + context.Command.Action);
            }
        }

        /// <summary>
        /// Gets the specific filters for the specified crud command.
        /// </summary>
        /// <param name="context">Context of the pipeline.</param>
        public void AddCrudSpecificActions(PipelineContext context)
        {
            switch (context.Command.Action)
            {
                case SpaceActionCommandEnum.Create:
                    this.AddCreateAction(context);
                    break;
                case SpaceActionCommandEnum.Alter:
                case SpaceActionCommandEnum.Drop:
                default:
                    throw new System.Exception("Not implemented crud command. Command: " + context.Command.Action);
            }
        }

        /// <summary>
        /// Add specific create actions.
        /// </summary>
        /// <param name="context">Context of the pipeline.</param>
        public void AddCreateAction(PipelineContext context)
        {
            switch (context.Command.SpaceObjectType)
            {
                case SpaceObjectEnum.Source:
                    context.Pipeline = new CreateSourceFilter();
                    break;
            }
        }

        /// <summary>
        /// Gets the specific filters for the specified status command.
        /// </summary>
        /// <param name="context">Context of the pipeline.</param>
        public void AddStatusSpecificActions(PipelineContext context)
        {
            switch (context.Command.Action)
            {
                case SpaceActionCommandEnum.Start:
                case SpaceActionCommandEnum.Stop:
                default:
                    throw new System.Exception("Not implemented status command. Command: " + context.Command.Action);
            }
        }

        /// <summary>
        /// Gets the specific filters for the specified permission command.
        /// </summary>
        /// <param name="context">Context of the pipeline.</param>
        public void AddPermissionsSpecificActions(PipelineContext context)
        {
            switch (context.Command.Action)
            {
                case SpaceActionCommandEnum.Grant:
                case SpaceActionCommandEnum.Revoke:
                case SpaceActionCommandEnum.Deny:
                default:
                    throw new System.Exception("Not implemented permission command. Command: " + context.Command.Action);
            }
        }

        /// <inheritdoc />
        public override void OnError(PipelineContext e)
        {
            throw new NotImplementedException();
        }        
    }
}
