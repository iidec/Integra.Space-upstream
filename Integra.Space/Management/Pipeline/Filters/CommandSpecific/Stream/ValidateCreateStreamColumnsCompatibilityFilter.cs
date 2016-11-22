//-----------------------------------------------------------------------
// <copyright file="ValidateCreateStreamColumnsCompatibilityFilter.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database;
    using Language;
    using Language.Runtime;
    using Ninject;

    /// <summary>
    /// Validate stream columns compatibility filter class.
    /// </summary>
    internal class ValidateCreateStreamColumnsCompatibilityFilter : CommandFilter
    {
        /// <inheritdoc />
        public override PipelineContext Execute(PipelineContext context)
        {
            CreateStreamNode command = (CreateStreamNode)context.CommandContext.Command;

            // obtengo la fuente especificada en la consulta.
            CommandObject source = command.OutputSource;

            // obtengo las columnas de la proyección del stream
            List<Tuple<string, Type>> projectionColumns = command.ExecutionPlan.GetQueryProyection();

            SpaceDbContext databaseContext = context.Kernel.Get<SpaceDbContext>();
            Login login = context.SecurityContext.Login;

            // obtengo el esquema de la fuente especificada en la consulta.
            Schema schema = source.GetSchema(databaseContext, login);

            // obtengo la fuente almacenada en la base de datos.
            Source sourceFromDatabase = databaseContext.Sources.Single(x => x.ServerId == schema.ServerId
                                            && x.DatabaseId == schema.DatabaseId
                                            && x.SchemaId == schema.SchemaId
                                            && x.SourceName == source.Name);

            // obtengo las columnas de la fuente
            Tuple<string, Type>[] sourceColumns = sourceFromDatabase.Columns.Select(x => Tuple.Create(x.ColumnName, Type.GetType(x.ColumnType))).ToArray();

            // verifico que todas las columnas de la proyeccion existan en la fuente especificada.
            foreach (Tuple<string, Type> projectionColumn in projectionColumns)
            {
                if (!sourceColumns.Any(x => x.Item1 == projectionColumn.Item1 && x.Item2 == projectionColumn.Item2))
                {
                    throw new Exception("Invalid projection column. All projection columns must exist as source columns.");
                }
            }

            return context;
        }

        /// <inheritdoc />
        public override void OnError(PipelineContext context)
        {
            base.OnError(context);
        }
    }
}
