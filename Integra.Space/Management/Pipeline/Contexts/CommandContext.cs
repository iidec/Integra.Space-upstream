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
    }
}
