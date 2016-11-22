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
    using System.Reflection.Emit;
    using Integra.Space.Database;
    using Language;
    using Language.Runtime;
    using Ninject;

    /// <summary>
    /// Create source type
    /// </summary>
    internal class CreateSourceTypeFilter : CommandFilter
    {
        /// <inheritdoc />
        public override PipelineContext Execute(PipelineContext context)
        {
            CreateSourceNode command = (CreateSourceNode)context.CommandContext.Command;
            Login login = context.SecurityContext.Login;
            SpaceDbContext databaseContext = context.Kernel.Get<SpaceDbContext>();
            Schema schema = command.MainCommandObject.GetSchema(databaseContext, login);
            Source source = databaseContext.Sources.Single(x => x.ServerId == schema.ServerId
                                            && x.DatabaseId == schema.DatabaseId
                                            && x.SchemaId == schema.SchemaId
                                            && x.SourceName == command.MainCommandObject.Name);

            IEnumerable<FieldNode> fields = source.Columns.Select(x => new FieldNode(x.ColumnName, Type.GetType(x.ColumnType)));

            string typeSignature = string.Format("{0}_{1}_{2}_{3}", schema.Database.Server.ServerName.Replace(' ', '\0'), schema.Database.DatabaseName.Replace(' ', '\0'), schema.SchemaName.Replace(' ', '\0'), source.SourceName.Replace(' ', '\0'));
            SpaceAssemblyBuilder sasmBuilder = new SpaceAssemblyBuilder(typeSignature);
            AssemblyBuilder asmBuilder = sasmBuilder.CreateAssemblyBuilder();
            SpaceModuleBuilder modBuilder = new SpaceModuleBuilder(asmBuilder);
            modBuilder.CreateModuleBuilder();
            SourceTypeBuilder typeBuilder = new SourceTypeBuilder(asmBuilder, typeSignature, typeof(object), fields);
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
