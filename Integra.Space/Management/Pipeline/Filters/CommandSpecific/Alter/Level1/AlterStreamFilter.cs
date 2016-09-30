//-----------------------------------------------------------------------
// <copyright file="AlterStreamFilter.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database;
    using Ninject;

    /// <summary>
    /// Filter alter user class.
    /// </summary>
    internal class AlterStreamFilter : AlterEntityFilter
    {
        /// <inheritdoc />
        protected override void EditEntity(PipelineContext context)
        {
            SpaceDbContext databaseContext = context.Kernel.Get<SpaceDbContext>();
            Stream stream = databaseContext.Streams.Single(x => x.ServerId == context.CommandContext.Schema.ServerId
                                            && x.DatabaseId == context.CommandContext.Schema.DatabaseId
                                            && x.SchemaId == context.CommandContext.Schema.SchemaId
                                            && x.StreamName == ((Language.DDLCommand)context.CommandContext.Command).MainCommandObject.Name);

            Dictionary<Common.StreamOptionEnum, object> options = ((Language.AlterStreamNode)context.CommandContext.Command).Options;

            if (options.ContainsKey(Common.StreamOptionEnum.Query) && !string.IsNullOrWhiteSpace(options[Common.StreamOptionEnum.Query] as string))
            {
                stream.Query = options[Common.StreamOptionEnum.Query].ToString();
            }

            if (options.ContainsKey(Common.StreamOptionEnum.Name))
            {
                stream.StreamName = options[Common.StreamOptionEnum.Name].ToString();
            }

            databaseContext.SaveChanges();
        }
    }
}
