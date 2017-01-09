//-----------------------------------------------------------------------
// <copyright file="AlterSourceFilter.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System.Collections.Generic;
    using System.Linq;
    using Database;

    /// <summary>
    /// Filter alter user class.
    /// </summary>
    internal class AlterSourceFilter : AlterEntityFilter<Language.AlterSourceNode, Common.SourceOptionEnum>
    {
        /// <inheritdoc />
        protected override void EditEntity(Language.AlterSourceNode command, Dictionary<Common.SourceOptionEnum, object> options, Schema schema, SpaceDbContext databaseContext)
        {
            Source source = databaseContext.Sources.Single(x => x.ServerId == schema.ServerId
                                            && x.DatabaseId == schema.DatabaseId
                                            && x.SchemaId == schema.SchemaId
                                            && x.SourceName == command.MainCommandObject.Name);
            
            if (options.ContainsKey(Common.SourceOptionEnum.Name))
            {
                source.SourceName = options[Common.SourceOptionEnum.Name].ToString();
            }

            if (command.Options.ContainsKey(Common.SourceOptionEnum.Status))
            {
                source.IsActive = (bool)command.Options[Common.SourceOptionEnum.Status];
            }

            databaseContext.SaveChanges();
        }
    }
}
