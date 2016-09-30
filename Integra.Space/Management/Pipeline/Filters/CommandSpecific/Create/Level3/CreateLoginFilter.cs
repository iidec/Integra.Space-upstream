//-----------------------------------------------------------------------
// <copyright file="CreateLoginFilter.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using System.Linq;
    using Database;
    using Language;
    using Ninject;

    /// <summary>
    /// Filter create source class.
    /// </summary>
    internal class CreateLoginFilter : CreateEntityFilter
    {
        /// <inheritdoc />
        protected override void CreateEntity(PipelineContext context)
        {
            CreateLoginNode loginCommand = (CreateLoginNode)context.CommandContext.Command;
            Schema schema = context.CommandContext.Schema;

            Login login = new Login();
            login.ServerId = schema.ServerId;
            login.LoginId = Guid.NewGuid();
            login.LoginName = loginCommand.MainCommandObject.Name;
            login.LoginPassword = loginCommand.Options[Common.LoginOptionEnum.Password].ToString();

            SpaceDbContext databaseContext = context.Kernel.Get<SpaceDbContext>();

            // se le establece la base de datos por defecto
            if (loginCommand.Options.ContainsKey(Common.LoginOptionEnum.Default_Database))
            {
                string databaseName = loginCommand.Options[Common.LoginOptionEnum.Default_Database].ToString();
                Database defaultDb = databaseContext.Databases.Single(x => x.ServerId == schema.ServerId && x.DatabaseName == databaseName);
                login.DefaultDatabaseServerId = defaultDb.ServerId;
                login.DefaultDatabaseId = defaultDb.DatabaseId;
            }
            else
            {
                login.DefaultDatabaseServerId = schema.ServerId;
                login.DefaultDatabaseId = schema.DatabaseId;
            }
                        
            // almaceno la nueva entidad y guardo los cambios
            databaseContext.Logins.Add(login);
            databaseContext.SaveChanges();
        }
    }
}
