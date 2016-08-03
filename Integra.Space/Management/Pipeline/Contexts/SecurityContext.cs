//-----------------------------------------------------------------------
// <copyright file="SecurityContext.cs" company="Integra.Space.common">
//     Copyright (c) Integra.Space.Language. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Management.Pipeline.Contexts
{
    using System.Collections.Generic;
    using System.Linq;
    using Database;
    using Integra.Space.Models;

    /// <summary>
    /// Security context class.
    /// </summary>
    internal sealed class SecurityContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityContext"/> class.
        /// </summary>
        /// <param name="user">Database user</param>
        public SecurityContext(DatabaseUser user)
        {
            this.User = user;

            using (SpaceDbContext context = new SpaceDbContext())
            {
                DatabaseUser userAux = context.DatabaseUsers
                    .Single(x => x.DbUsrId == user.DbUsrId && x.DatabaseId == user.DatabaseId && x.ServerId == user.ServerId);

                this.DatabaseRoles = userAux.DatabaseRoles;
                this.Logins = userAux.Logins;
            }
        }

        /// <summary>
        /// Gets the user requesting the command execution.
        /// </summary>
        public DatabaseUser User { get; private set; }

        /// <summary>
        /// Gets the database roles of the user.
        /// </summary>
        public ICollection<DatabaseRole> DatabaseRoles { get; private set; }

        /// <summary>
        /// Gets the server roles of the user.
        /// </summary>
        public ICollection<ServerRole> ServerRoles { get; private set; }

        /// <summary>
        /// Gets the logins of the user.
        /// </summary>
        public ICollection<Login> Logins { get; private set; }
    }
}
