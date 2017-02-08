//-----------------------------------------------------------------------
// <copyright file="CreateSourceTypeFilter.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Compiler;
    using Database;
    using Language;
    using Ninject;

    /// <summary>
    /// Create source type
    /// </summary>
    internal class CreateSourceTypeFilter : CommandFilter
    {
        /// <inheritdoc />
        public override PipelineContext Execute(PipelineContext context)
        {
            SystemCommand command = context.CommandContext.Command;
            Login login = context.SecurityContext.Login;
            SpaceDbContext databaseContext = context.Kernel.Get<SpaceDbContext>();
            Schema schema = command.MainCommandObject.GetSchema(databaseContext, login);
            Source source = databaseContext.Sources.Single(x => x.ServerId == schema.ServerId
                                                                    && x.DatabaseId == schema.DatabaseId 
                                                                    && x.SchemaId == schema.SchemaId
                                                                    && x.SourceName == command.MainCommandObject.Name);
            
            string typeSignature = string.Format("{0}_{1}", context.AssemblyBuilder.GetName(), source.SourceName); // string.Format("{0}_{1}_{2}_{3}", schema.Database.Server.ServerName.Replace(' ', '\0'), schema.Database.DatabaseName.Replace(' ', '\0'), schema.SchemaName.Replace(' ', '\0'), source.SourceName.Replace(' ', '\0'));

            IEnumerable<FieldNode> fields = source.Columns.Select(x => new FieldNode(x.ColumnName, Type.GetType(x.ColumnType)));
            
            SourceTypeBuilder typeBuilder = new SourceTypeBuilder(context.AssemblyBuilder, typeSignature, typeof(object), fields);
            Type newType = typeBuilder.CreateNewType();

            return context;
        }

        /// <inheritdoc />
        public override void OnError(PipelineContext context)
        {
            base.OnError(context);
        }
    }
}
