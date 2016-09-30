//-----------------------------------------------------------------------
// <copyright file="SpecificFiltersAggregator.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using System.Linq;
    using Common;
    using Database;
    using Language;
    using Ninject;

    /// <summary>
    /// Specific filters aggregator class.
    /// </summary>
    internal class SpecificFiltersAggregator : FirstLevelCommandFilter
    {
        /// <inheritdoc />
        public override FirstLevelPipelineContext Execute(FirstLevelPipelineContext context)
        {
            foreach (CommandPipelineNode commandNode in context.Commands)
            {
                if (commandNode.Command is DDLCommand)
                {
                    this.AddSpecificFiltersToDDLCommand(context, commandNode);
                }
                else if (commandNode.Command is DMLCommand)
                {
                    this.AddSpecificFiltersToDMLCommand(context, commandNode);
                }
                else
                {
                    throw new Exception("Invalid command type. Must be a DDL or DML command.");
                }
            }

            return context;
        }

        /// <inheritdoc />
        public override void OnError(FirstLevelPipelineContext e)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Add specific filters to a DML command.
        /// </summary>
        /// <param name="context">Fist level pipeline context.</param>
        /// <param name="commandNode">Compile command node.</param>
        private void AddSpecificFiltersToDMLCommand(FirstLevelPipelineContext context, CommandPipelineNode commandNode)
        {
            ActionCommandEnum action = commandNode.Command.Action;
            if (commandNode.Command is QueryCommandForMetadataNode)
            {
                QueryCommandForMetadataNode dmlCommand = (QueryCommandForMetadataNode)commandNode.Command;
                PlanNode fromNode = Language.Runtime.NodesFinder.FindNode(dmlCommand.ExecutionPlan, new PlanNodeTypeEnum[] { PlanNodeTypeEnum.ObservableFrom }).First();
                Type typeItemSource = ((Type)fromNode.Properties["SourceType"]).GetGenericArguments()[0];

                if (typeItemSource == typeof(Server))
                {
                    commandNode.Pipeline = SpecificFilterSelector.GetSpecificFilter(new SpecificFilterKey(action, SystemObjectEnum.Server));
                }
                else if (typeItemSource == typeof(Stream))
                {
                    commandNode.Pipeline = SpecificFilterSelector.GetSpecificFilter(new SpecificFilterKey(action, SystemObjectEnum.Stream));
                }
            }
        }

        /// <summary>
        /// Add specific filters to a DDL command.
        /// </summary>
        /// <param name="context">Fist level pipeline context.</param>
        /// <param name="commandNode">Compile command node.</param>
        private void AddSpecificFiltersToDDLCommand(FirstLevelPipelineContext context, CommandPipelineNode commandNode)
        {
            ActionCommandEnum action = commandNode.Command.Action;
            DDLCommand ddlCommand = (DDLCommand)commandNode.Command;
            if (action == ActionCommandEnum.Grant || action == ActionCommandEnum.Deny || action == ActionCommandEnum.Revoke)
            {
                PermissionsCommandNode command = (PermissionsCommandNode)ddlCommand;
                SpaceDbContext databaseContext = context.Kernel.Get<SpaceDbContext>();
                if (command.Permission.ObjectType == null || command.Permission.ObjectName == null)
                {
                    string securableClassName = databaseContext.GranularPermissions
                        .Single(gp => gp.GranularPermissionName.Replace(" ", string.Empty).Equals(command.Permission.Permission.ToString(), StringComparison.InvariantCultureIgnoreCase))
                        .PermissionsBySecurables
                        .Single()
                        .SecurableClass
                        .SecurableName;

                    SystemObjectEnum objectType;
                    if (!Enum.TryParse(securableClassName, true, out objectType))
                    {
                        throw new Exception("Securable class not defined.");
                    }

                    // si es un permiso donde no se especifica el objeto se debe tomar del contexto del comando.
                    switch (objectType)
                    {
                        case SystemObjectEnum.Database:
                            command.Permission.ObjectType = (SystemObjectEnum)Enum.Parse(typeof(SystemObjectEnum), "database", true);
                            break;
                        case SystemObjectEnum.Server:
                            command.Permission.ObjectType = (SystemObjectEnum)Enum.Parse(typeof(SystemObjectEnum), "server", true);
                            break;
                        default:
                            throw new Exception(string.Format("System object not allowed for permission {0}.", command.Permission.Permission.ToString()));
                    }
                }

                commandNode.Pipeline = SpecificFilterSelector.GetSpecificFilter(command.Permission.ObjectType.Value);
            }
            else if (action == ActionCommandEnum.Create || action == ActionCommandEnum.Alter || action == ActionCommandEnum.Drop)
            {
                commandNode.Pipeline = SpecificFilterSelector.GetSpecificFilter(new SpecificFilterKey(action, ddlCommand.MainCommandObject.SecurableClass));
            }
            else if (action == ActionCommandEnum.TakeOwnership)
            {
                commandNode.Pipeline = SpecificFilterSelector.GetSpecificFilter(new SpecificFilterKey(ActionCommandEnum.TakeOwnership, ddlCommand.MainCommandObject.SecurableClass));
            }
            else if (action == ActionCommandEnum.Add)
            {
                commandNode.Pipeline = new AddSecureObjectToRoleFilter();
            }
        }
    }
}
