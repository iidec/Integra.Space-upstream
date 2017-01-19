//-----------------------------------------------------------------------
// <copyright file="AlterViewFilter.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using System.Linq;
    using Database;
    using Ninject;
    using System.Collections.Generic;

    /// <summary>
    /// Filter alter user class.
    /// </summary>
    internal class AlterViewFilter : AlterEntityFilter
    {
        /// <inheritdoc />
        protected override void EditEntity(PipelineContext context)
        {
            View view = databaseContext.Views.Single(x => x.ServerId == context.CommandContext.Schema.ServerId
                                            && x.DatabaseId == context.CommandContext.Schema.DatabaseId
                                            && x.SchemaId == context.CommandContext.Schema.SchemaId
                                            && x.ViewName == ((Language.DDLCommand)context.CommandContext.Command).MainCommandObject.Name);

            string predicate = string.Empty;
            
            if (string.IsNullOrWhiteSpace(predicate))
            {
                view.Predicate = predicate;
            }
            else
            {
                throw new Exception("Must specify the predicate for the view.");
            }

            databaseContext.SaveChanges();
        }
    }
}
