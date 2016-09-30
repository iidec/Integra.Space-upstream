//-----------------------------------------------------------------------
// <copyright file="PermissionFilter.cs" company="Integra.Space.Language">
//     Copyright (c) Integra.Space.Language. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using Common;
    using Database;
    using Integra.Space.Language;

    /// <summary>
    /// Grant permission filter class.
    /// </summary>
    internal abstract class PermissionFilter : CommandFilter
    {
        /// <inheritdoc />
        public override PipelineContext Execute(PipelineContext context)
        {
            Schema schema = context.CommandContext.Schema;
            PermissionsCommandNode command = (PermissionsCommandNode)context.CommandContext.Command;
            PermissionNode permission = command.Permission;

            DatabaseAssignedPermissionsToUser p1 = new DatabaseAssignedPermissionsToUser();
            foreach (CommandObject principal in command.Principals)
            {
                if (principal.SecurableClass.Equals(SystemObjectEnum.DatabaseUser) || principal.SecurableClass.Equals(SystemObjectEnum.Login))
                {
                    this.SavePermissionForUser(principal, context, schema, command, permission);
                }
                else if (principal.SecurableClass.Equals(SystemObjectEnum.DatabaseRole))
                {
                    this.SavePermissionForRole(principal, context, schema, command, permission);
                }
            }

            return context;
        }

        /// <inheritdoc />
        public override void OnError(PipelineContext context)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Saves a new permission for the specified role.
        /// </summary>
        /// <param name="user">Principal to assign the permission.</param>
        /// <param name="context">Pipeline context.</param>
        /// <param name="schema">Schema of the command.</param>
        /// <param name="command">Command to execute.</param>
        /// <param name="permission">Permission to assign.</param>
        protected abstract void SavePermissionForUser(CommandObject user, PipelineContext context, Schema schema, PermissionsCommandNode command, PermissionNode permission);

        /// <summary>
        /// Saves a new permission for the specified role.
        /// </summary>
        /// <param name="role">Principal to assign the permission.</param>
        /// <param name="context">Pipeline context.</param>
        /// <param name="schema">Schema of the command.</param>
        /// <param name="command">Command to execute.</param>
        /// <param name="permission">Permission to assign.</param>
        protected abstract void SavePermissionForRole(CommandObject role, PipelineContext context, Schema schema, PermissionsCommandNode command, PermissionNode permission);        
    }
}
