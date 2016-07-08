//-----------------------------------------------------------------------
// <copyright file="SpecificFiltersAggregator.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using Common;
    using Language;

    /// <summary>
    /// Specific filters aggregator class.
    /// </summary>
    internal class SpecificFiltersAggregator : FirstPipelineFilter
    {
        /// <inheritdoc />
        public override PipelineContext Execute(PipelineContext input)
        {
            if ((SpaceActionCommandEnum.CrudCommands & input.Command.Action) == SpaceActionCommandEnum.Create
                 || (SpaceActionCommandEnum.CrudCommands & input.Command.Action) == SpaceActionCommandEnum.Alter
                 || (SpaceActionCommandEnum.CrudCommands & input.Command.Action) == SpaceActionCommandEnum.Drop)
            {
                this.AddCrudSpecificActions(input);
            }
            else if ((SpaceActionCommandEnum.StatusCommands & input.Command.Action) == SpaceActionCommandEnum.Start
                || (SpaceActionCommandEnum.StatusCommands & input.Command.Action) == SpaceActionCommandEnum.Stop)
            {
                this.AddStatusSpecificActions(input);
            }
            else if ((SpaceActionCommandEnum.CommandsPermissions & input.Command.Action) == SpaceActionCommandEnum.Grant
                || (SpaceActionCommandEnum.CommandsPermissions & input.Command.Action) == SpaceActionCommandEnum.Deny
                || (SpaceActionCommandEnum.CommandsPermissions & input.Command.Action) == SpaceActionCommandEnum.Revoke)
            {
                this.AddPermissionsSpecificActions(input);
            }
            else
            {
                throw new System.Exception("Not implement command. Command: " + input.Command.Action);
            }

            return input;
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
                    context.Pipeline = new CreateSourceFilter();
                    break;
                case SpaceActionCommandEnum.Alter:
                case SpaceActionCommandEnum.Drop:
                default:
                    throw new System.Exception("Not implemented crud command. Command: " + context.Command.Action);
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
