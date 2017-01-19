//-----------------------------------------------------------------------
// <copyright file="ValidateAlterStreamOutputColumnsCompatibilityFilter.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using System.Collections.Generic;
    using Common;
    using Database;
    using Language;
    using Ninject;

    /// <summary>
    /// Validate stream columns compatibility filter class.
    /// </summary>
    internal class ValidateAlterStreamOutputColumnsCompatibilityFilter : OutputColumnsCompatibilityValidatorFilter
    {
        /// <inheritdoc />
        public override PipelineContext Execute(PipelineContext context)
        {
            AlterStreamNode command = (AlterStreamNode)context.CommandContext.Command;
            
            // obtengo las columnas de la proyección del stream
            if (command.ExecutionPlan != null)
            {
                SpaceDbContext databaseContext = context.Kernel.Get<SpaceDbContext>();
                Login login = context.SecurityContext.Login;

                this.ValidateSources(command.OutputSource, command.ExecutionPlan, command.InputSources, databaseContext, login);
            }

            return context;
        }

        /// <inheritdoc />
        public override void OnError(PipelineContext context)
        {
            base.OnError(context);
        }

        /// <inheritdoc />
        protected override List<Source> GetInputSources(ReferencedSource[] inputSources, SpaceDbContext databaseContext, Login login)
        {
            List<Source> inputSourcesAux = new List<Source>();
            foreach (var inputSource in inputSources)
            {
                // obtengo la fuente almacenada en la base de datos.
                Source inputSourceFromDatabase = this.GetSourceFromDatabase(new CommandObject(SystemObjectEnum.Source, inputSource.DatabaseName, inputSource.SchemaName, inputSource.SourceName, PermissionsEnum.None, false), databaseContext, login);

                if (inputSourceFromDatabase == null)
                {
                    throw new Exception(string.Format("Invalid source input source '{0}'.", inputSource.SourceName));
                }
                else
                {
                    inputSourcesAux.Add(inputSourceFromDatabase);
                }
            }

            return inputSourcesAux;
        }
    }
}
