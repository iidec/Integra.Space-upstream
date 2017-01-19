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
                    sourceType = typeof(ServerView);
                    break;
                case SystemSourceEnum.ServerRoles:
                    sourceType = typeof(ServerRoleView);
                    break;
                case SystemSourceEnum.Logins:
                    sourceType = typeof(LoginView);
                    break;
                case SystemSourceEnum.Databases:
                    sourceType = typeof(DatabaseView);
                    break;
                case SystemSourceEnum.DatabaseRoles:
                    sourceType = typeof(DatabaseRoleView);
                    break;
                case SystemSourceEnum.Users:
                    sourceType = typeof(UserView);
                    break;
                case SystemSourceEnum.Schemas:
                    sourceType = typeof(SchemaView);
                    break;
                case SystemSourceEnum.Sources:
                    sourceType = typeof(SourceView);
                    break;
                case SystemSourceEnum.SourceColumns:
                    sourceType = typeof(SourceColumnView);
                    break;
                case SystemSourceEnum.Streams:
                    sourceType = typeof(StreamView);
                    break;
                case SystemSourceEnum.StreamColumns:
                    sourceType = typeof(StreamColumnView);
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
