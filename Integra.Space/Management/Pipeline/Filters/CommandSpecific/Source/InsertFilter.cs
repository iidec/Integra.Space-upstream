//-----------------------------------------------------------------------
// <copyright file="InsertFilter.cs" company="Integra.Space">
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
    using Ninject;

    /// <summary>
    /// Truncate command action class.
    /// </summary>
    internal class InsertFilter : CommandFilter
    {
        /// <inheritdoc />
        public override PipelineContext Execute(PipelineContext context)
        {
            Language.InsertNode command = (Language.InsertNode)context.CommandContext.Command;
            SpaceDbContext databaseContext = context.Kernel.Get<SpaceDbContext>();
            Login login = context.SecurityContext.Login;
            Schema schema = command.MainCommandObject.GetSchema(databaseContext, login);
            Source source = databaseContext.Sources.Single(x => x.ServerId == schema.ServerId
                                            && x.DatabaseId == schema.DatabaseId
                                            && x.SchemaId == schema.SchemaId
                                            && x.SourceName == command.MainCommandObject.Name);
            
            List<Tuple<byte, object>> listOfValues = new List<Tuple<byte, object>>();

            // valido que todas las columnas de la fuente estan definidas en las columnas del insert.
            foreach (SourceColumn column in source.Columns)
            {
                listOfValues.Add(Tuple.Create(column.ColumnIndex, command.ColumnsWithValues[column.ColumnName]));
            }

            ISourceTypeFactory sourceFactory = context.Kernel.Get<ISourceTypeFactory>();
            object[] arrayOfValues = listOfValues.OrderBy(x => x.Item1).Select(x => x.Item2).ToArray();

            object @event = Activator.CreateInstance(sourceFactory.GetSourceType(command.MainCommandObject), arrayOfValues);

            /* Aqui se enviará el evento por un buffer block, esta sección solo se utilizará para efectos de pruebas y se tendrá que eliminar */
            ISource targetSource = context.Kernel.Get<ISource>();
            targetSource.Enqueue((EventBase)@event);

            return context;
        }
    }
}