//-----------------------------------------------------------------------
// <copyright file="AlterDatabaseFilter.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System.Collections.Generic;
    using System.Linq;
    using Database;
    using Ninject;

    /// <summary>
    /// Filter alter user class.
    /// </summary>
    internal class AlterDatabaseFilter : AlterEntityFilter<Language.AlterDatabaseNode, Common.DatabaseOptionEnum>
    {
        /// <inheritdoc />
        protected override void EditEntity(Language.AlterDatabaseNode command, Dictionary<Common.DatabaseOptionEnum, object> options, Schema schema, SpaceDbContext databaseContext)
        {
            Database database = databaseContext.Databases.Single(x => x.ServerId == schema.ServerId
                                            && x.DatabaseName == command.MainCommandObject.Name);
            
            if (options.ContainsKey(Common.DatabaseOptionEnum.Name))
            {
                database.DatabaseName = options[Common.DatabaseOptionEnum.Name].ToString();
            }

            if (command.Options.ContainsKey(Common.DatabaseOptionEnum.Status))
            {
                database.IsActive = (bool)command.Options[Common.DatabaseOptionEnum.Status];
            }

            databaseContext.SaveChanges();
        }
    }
}
