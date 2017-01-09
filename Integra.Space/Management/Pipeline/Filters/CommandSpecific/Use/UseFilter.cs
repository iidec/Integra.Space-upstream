//-----------------------------------------------------------------------
// <copyright file="UseFilter.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System.Linq;
    using Database;
    using Ninject;

    /// <summary>
    /// Filter create source class.
    /// </summary>
    internal class UseFilter : CommandFilter
    {
        /// <inheritdoc />
        public override PipelineContext Execute(PipelineContext context)
        {
            Language.UseCommandNode command = (Language.UseCommandNode)context.CommandContext.Command;
            Login login = context.SecurityContext.Login;
            SpaceDbContext databaseContext = context.Kernel.Get<SpaceDbContext>();
            Schema schema = command.MainCommandObject.GetSchema(databaseContext, login);

            bool exists = databaseContext.DatabaseUsers.Any(x => x.ServerId == schema.ServerId && x.DatabaseId == schema.DatabaseId && x.LoginServerId == login.ServerId && x.LoginId == login.LoginId);
            if (!exists)
            {
                throw new System.Exception(string.Format("Not user mapped at database '{0}' for login '{1}'", command.MainCommandObject.Name, login.LoginName));
            }

            return context;
        }

        /// <inheritdoc />
        public override void OnError(PipelineContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}
