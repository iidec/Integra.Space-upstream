//-----------------------------------------------------------------------
// <copyright file="ValidateMetadataOutputColumnsCompatibilityFilter.cs" company="Integra.Space">
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
    internal class ValidateMetadataOutputColumnsCompatibilityFilter : OutputColumnsCompatibilityValidatorFilter
    {
        /// <inheritdoc />
        public override PipelineContext Execute(PipelineContext context)
        {
            QueryCommandForMetadataNode command = (QueryCommandForMetadataNode)context.CommandContext.Command;
            SpaceDbContext databaseContext = context.Kernel.Get<SpaceDbContext>();
            Login login = context.SecurityContext.Login;

            this.ValidateSources(command.OutputSource, command.ExecutionPlan, command.InputSources, databaseContext, login);

            return context;
        }

        /// <inheritdoc />
        public override void OnError(PipelineContext context)
        {
            base.OnError(context);
        }

        /// <summary>
        /// Generates the source from a metadata source type.
        /// </summary>
        /// <param name="systemSource">Metadata source type.</param>
        /// <returns>Integra.Space.Database.Source object representing the metadata source.</returns>
        protected Source GenerateMetadataSource(SystemSourceEnum systemSource)
        {
            Source s = new Source();
            s.SourceName = systemSource.ToString();

            Type sourceType = null;

            switch (systemSource)
            {
                case SystemSourceEnum.Servers:
                    sourceType = typeof(Space.Database.Server);
                    break;
                case SystemSourceEnum.ServerRoles:
                    sourceType = typeof(Space.Database.ServerRole);
                    break;
                case SystemSourceEnum.Logins:
                    sourceType = typeof(Space.Database.Login);
                    break;
                case SystemSourceEnum.Databases:
                    sourceType = typeof(Space.Database.Database);
                    break;
                case SystemSourceEnum.DatabaseRoles:
                    sourceType = typeof(Space.Database.DatabaseRole);
                    break;
                case SystemSourceEnum.Users:
                    sourceType = typeof(Space.Database.DatabaseUser);
                    break;
                case SystemSourceEnum.Schemas:
                    sourceType = typeof(Space.Database.Schema);
                    break;
                case SystemSourceEnum.Sources:
                    sourceType = typeof(Space.Database.Source);
                    break;
                case SystemSourceEnum.SourceColumns:
                    sourceType = typeof(Space.Database.SourceColumn);
                    break;
                case SystemSourceEnum.Streams:
                    sourceType = typeof(Space.Database.Stream);
                    break;
                case SystemSourceEnum.StreamColumns:
                    sourceType = typeof(Space.Database.StreamColumn);
                    break;
            }

            foreach (var prop in sourceType.GetProperties())
            {
                SourceColumn sc = new SourceColumn();
                sc.ColumnName = prop.Name;
                sc.ColumnType = prop.PropertyType.AssemblyQualifiedName;
                s.Columns.Add(sc);
            }

            return s;
        }

        /// <inheritdoc />
        protected override List<Source> GetInputSources(ReferencedSource[] inputSources, SpaceDbContext databaseContext, Login login)
        {
            List<Source> inputSourcesAux = new List<Source>();
            foreach (var inputSource in inputSources)
            {
                SystemSourceEnum systemSource;
                if (Enum.TryParse(inputSource.SourceName, true, out systemSource))
                {
                    inputSourcesAux.Add(this.GenerateMetadataSource(systemSource));
                }
                else
                {
                    throw new Exception(string.Format("Invalid source input source '{0}'.", inputSource.SourceName));
                }
            }

            return inputSourcesAux;
        }

        /// <summary>
        /// Source column class.
        /// </summary>
        private class SourceColumnAux
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SourceColumnAux"/> class.
            /// </summary>
            /// <param name="propertyName">Property name.</param>
            /// <param name="propertyType">Property type.</param>
            public SourceColumnAux(string propertyName, Type propertyType)
            {
                this.PropertyName = propertyName;
                this.PropertyType = propertyType;
            }

            /// <summary>
            /// Gets the property name.
            /// </summary>
            public string PropertyName { get; private set; }

            /// <summary>
            /// Gets the property type.
            /// </summary>
            public Type PropertyType { get; private set; }
        }
    }
}
