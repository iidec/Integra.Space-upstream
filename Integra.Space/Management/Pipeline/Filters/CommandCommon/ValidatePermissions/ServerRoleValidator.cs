//-----------------------------------------------------------------------
// <copyright file="ServerRoleValidator.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using System.Linq;
    using Database;
    using Integra.Space.Common;

    /// <summary>
    /// Server role validator.
    /// </summary>
    internal class ServerRoleValidator
    {
        /// <summary>
        /// Database context.
        /// </summary>
        private SpaceDbContext databaseContext;

        /// <summary>
        /// Login used in the execution context.
        /// </summary>
        private Login login;

        /// <summary>
        /// Action to perform by the command.
        /// </summary>
        private ActionCommandEnum action;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerRoleValidator"/> class.
        /// </summary>
        /// <param name="databaseContext">Database context.</param>
        /// <param name="login">Login used in the execution context.</param>
        /// <param name="action">Action to perform by the command.</param>
        public ServerRoleValidator(SpaceDbContext databaseContext, Login login, ActionCommandEnum action)
        {
            this.databaseContext = databaseContext;
            this.login = login;
            this.action = action;
        }

        /// <summary>
        /// Verify if the login belongs to a server role.
        /// </summary>
        /// <returns>A value indicating whether the login belongs to a server role.</returns>
        public bool BelongsToServerRole()
        {
            // verifica si pertenece al rol del sistema sysadmin.
            bool isSysAdmin = this.databaseContext.ServerRoles
                .Single(x => SystemRolesEnum.SysAdmin.ToString().Equals(x.ServerRoleName.ToString(), StringComparison.InvariantCultureIgnoreCase))
                .Logins
                .Any(x => x.ServerId == this.login.ServerId && x.LoginId == this.login.LoginId);

            if (isSysAdmin)
            {
                return true;
            }

            // verifica si pertenece al rol del sistema sysreader.
            bool isSysReader = this.databaseContext.ServerRoles
                .Single(x => SystemRolesEnum.SysReader.ToString().Equals(x.ServerRoleName.ToString(), StringComparison.InvariantCultureIgnoreCase))
                .Logins
                .Any(x => x.ServerId == this.login.ServerId && x.LoginId == this.login.LoginId);

            if (isSysReader && this.action == ActionCommandEnum.Read)
            {
                return true;
            }

            return false;
        }
    }
}
