//-----------------------------------------------------------------------
// <copyright file="CommandContext.cs" company="Integra.Space.common">
//     Copyright (c) Integra.Space.Language. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline
{
    using System.Diagnostics.Contracts;
    using System.Linq;
    using Database;
    using Language;
    using Ninject;

    /// <summary>
    /// Command context class.
    /// </summary>
    internal class CommandContext
    {
        /// <summary>
        /// Space command.
        /// </summary>
        private SystemCommand command;

        /// <summary>
        /// The schema where the command will be executed.
        /// </summary>
        private Space.Database.Schema schema;

        /// <summary>
        /// Login of the client.
        /// </summary>
        private Login login;

        /// <summary>
        /// DI kernel.
        /// </summary>
        private IKernel kernel;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandContext"/> class.
        /// </summary>
        /// <param name="commandString">Command string.</param>
        /// <param name="command">Compiled command.</param>
        /// <param name="login">Login of the client.</param>
        /// <param name="kernel">DI kernel.</param>
        public CommandContext(string commandString, SystemCommand command, Login login, IKernel kernel)
        {
            Contract.Assert(!string.IsNullOrWhiteSpace(commandString));
            Contract.Assert(command != null);
            Contract.Assert(kernel != null);

            this.CommandString = commandString;
            this.command = command;
            this.login = login;
            this.kernel = kernel;
        }

        /// <summary>
        /// Gets the space command string.
        /// </summary>
        public string CommandString { get; private set; }

        /// <summary>
        /// Gets or sets the space command.
        /// </summary>
        public SystemCommand Command
        {
            get
            {
                return this.command;
            }

            set
            {
                if (this.command == null)
                {
                    this.command = value;
                }
            }
        }

        /// <summary>
        /// Gets the schema where the command will be executed.
        /// </summary>
        public Space.Database.Schema Schema
        {
            get
            {
                if (this.schema == null)
                {
                    this.schema = this.GetSchema();
                }

                return this.schema;
            }
        }

        /// <summary>
        /// Gets the schema for the actual command execution.
        /// </summary>
        /// <returns>Schema of the command.</returns>
        private Space.Database.Schema GetSchema()
        {
            Contract.Assert(this.command != null);

            SpaceDbContext databaseContext = this.kernel.Get<SpaceDbContext>();
            Database databaseAux = null;
            if (this.command.DatabaseName != null)
            {
                databaseAux = databaseContext.Databases.Single(x => x.ServerId == this.login.ServerId && x.DatabaseName == this.command.DatabaseName);
            }
            else
            {
                databaseAux = databaseContext.Databases.Single(x => x.ServerId == this.login.DefaultDatabaseServerId && x.DatabaseId == this.login.DefaultDatabaseId);
            }

            Space.Database.Schema schemaAux = null;
            if (this.command.SchemaName != null)
            {
                if (databaseAux.Schemas.Select(x => x.SchemaName).Contains(this.command.SchemaName))
                {
                    schemaAux = databaseAux.Schemas.Single(x => x.SchemaName == this.command.SchemaName);
                }
                else
                {
                    throw new System.Exception(string.Format("The schema does not exist in the database: '{0}'.", databaseAux.DatabaseName));
                }
            }
            else
            {
                DatabaseUser userAux = null;

                try
                {
                    // obtengo el usuario que quiere ejecutar el comando. Este tambien se calcula en la clase SecureContextBuilderFilter
                    userAux = this.login.DatabaseUsers.Single(x => x.ServerId == databaseAux.ServerId && x.DatabaseId == databaseAux.DatabaseId);
                }
                catch (System.Exception e)
                {
                    throw new System.Exception(string.Format("No user mapped at the database '{0}' for the login '{1}'.", databaseAux.DatabaseName, this.login.LoginName));
                }

                // obtengo el esquema de ejecución.
                schemaAux = databaseContext.DatabaseUsers.Single(x => x.DatabaseId == databaseAux.DatabaseId && x.ServerId == databaseAux.ServerId && x.DbUsrName == userAux.DbUsrName).DefaultSchema;
            }

            return schemaAux;
        }
    }
}
