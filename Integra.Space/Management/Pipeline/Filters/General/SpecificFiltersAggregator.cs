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

                SystemObjectEnum objectType;
                if (Enum.TryParse(typeItemSource.Name, true, out objectType))
                {
                    commandNode.Pipeline = SpecificFilterSelector.GetSpecificFilter(new SpecificFilterKey(action, objectType));
                }
                else
                {
                    throw new Exception(string.Format("The system type '{0}' does not exist.", typeItemSource.Name));
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
            /* NOTA: el comando use <database> no tiene acciones específicas. */
            ActionCommandEnum action = commandNode.Command.Action;
            DDLCommand ddlCommand = (DDLCommand)commandNode.Command;
            if (action == ActionCommandEnum.Grant || action == ActionCommandEnum.Deny || action == ActionCommandEnum.Revoke)
            {
                PermissionsCommandNode command = (PermissionsCommandNode)ddlCommand;
                SpaceDbContext databaseContext = context.Kernel.Get<SpaceDbContext>();
                SystemObjectEnum objectType;
                if (command.Permission.CommandObject == null)
                {
                    string securableClassName = databaseContext.GranularPermissions
                        .Single(gp => gp.GranularPermissionName.Replace(" ", string.Empty).Equals(command.Permission.Permission.ToString(), StringComparison.InvariantCultureIgnoreCase))
                        .PermissionsBySecurables
                        .Single()
                        .SecurableClass
                        .SecurableName;

                    if (!Enum.TryParse(securableClassName, true, out objectType))
                    {
                        throw new Exception("Securable class not defined.");
                    }

                    // se obtiene el login para obtener el server o base de datos por defecto.
                    Login login = context.Kernel.Get<SpaceDbContext>().Logins.Single(x => x.LoginName == context.Login);

                    // si es un permiso donde no se especifica el objeto se debe tomar del contexto del comando.
                    switch (objectType)
                    {
                        case SystemObjectEnum.Database:
                            command.Permission.CommandObject = new CommandObject(objectType, login.Database.DatabaseName, PermissionsEnum.Control, false);
                            break;
                        case SystemObjectEnum.Server:
                            command.Permission.CommandObject = new CommandObject(objectType, login.Server.ServerName, PermissionsEnum.Control, false);
                            break;
                        default:
                            throw new Exception(string.Format("System object not allowed for permission {0}.", command.Permission.Permission.ToString()));
                    }
                }
                else
                {
                    objectType = command.Permission.CommandObject.SecurableClass;
                }

                commandNode.Pipeline = SpecificFilterSelector.GetSpecificFilter(objectType);
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
            else if (action == ActionCommandEnum.Remove)
            {
                commandNode.Pipeline = new RemoveSecureObjectToRoleFilter();
            }
        }
    }
}
