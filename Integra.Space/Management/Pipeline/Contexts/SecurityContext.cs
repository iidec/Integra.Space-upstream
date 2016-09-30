//-----------------------------------------------------------------------
// <copyright file="SecurityContext.cs" company="Integra.Space.common">
//     Copyright (c) Integra.Space.Language. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline
{
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using Database;
    using Ninject;

    /// <summary>
    /// Security context class.
    /// </summary>
    internal sealed class SecurityContext
    {
        /// <summary>
        /// The user requesting the command execution.
        /// </summary>
        private DatabaseUser user;

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityContext"/> class.
        /// </summary>
        /// <param name="login">Login that will execute the command.</param>
        /// <param name="kernel">DI kernel.</param>
        public SecurityContext(string login, IKernel kernel)
        {
            Contract.Assert(!string.IsNullOrWhiteSpace(login));

            // obtengo el contexto de base de datos
            SpaceDbContext context = kernel.Get<SpaceDbContext>();
            
            // se obtiene el login definido para ejecutar el comando.
            this.Login = context.Logins.Single(x => x.LoginName == login);
            
            // se obtienen los roles del sistema para el login.
            this.ServerRoles = this.Login.ServerRoles;
        }

        /// <summary>
        /// Gets or sets the user requesting the command execution.
        /// </summary>
        public DatabaseUser User
        {
            get
            {
                return this.user;
            }

            set
            {
                if (this.user == null)
                {
                    this.user = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the database roles of the user.
        /// </summary>
        public ICollection<DatabaseRole> DatabaseRoles { get; set; }

        /// <summary>
        /// Gets the server roles of the user.
        /// </summary>
        public ICollection<ServerRole> ServerRoles { get; private set; }

        /// <summary>
        /// Gets the logins of the user.
        /// </summary>
        public Login Login { get; private set; }
    }
}
