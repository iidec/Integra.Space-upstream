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
    internal class AlterDatabaseFilter : AlterEntityFilter
    {
        /// <inheritdoc />
        protected override void EditEntity(PipelineContext context)
        {
            Dictionary<Common.DatabaseOptionEnum, object> options = ((Language.CreateObjectNode<Common.DatabaseOptionEnum>)context.CommandContext.Command).Options;

            SpaceDbContext databaseContext = context.Kernel.Get<SpaceDbContext>();
            Database database = databaseContext.Databases.Single(x => x.ServerId == context.CommandContext.Schema.ServerId
                                            && x.DatabaseName == ((Language.DDLCommand)context.CommandContext.Command).MainCommandObject.Name);
            
            if (options.ContainsKey(Common.DatabaseOptionEnum.Name))
            {
                database.DatabaseName = options[Common.DatabaseOptionEnum.Name].ToString();
            }

            databaseContext.SaveChanges();
        }
    }
}
