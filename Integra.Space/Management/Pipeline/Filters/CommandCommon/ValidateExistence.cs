//-----------------------------------------------------------------------
// <copyright file="ValidateExistence.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Common;
    using Database;
    using Ninject;

    /// <summary>
    /// Class to validate whether the existence of the specified space objects applies to the command.
    /// </summary>
    internal class ValidateExistence : CommandFilter
    {
        /// <inheritdoc />
        public override PipelineContext Execute(PipelineContext context)
        {
            // acción del comando.
            ActionCommandEnum action = context.CommandContext.Command.Action;

            /*
            HashSet<CommandObject> objects = new HashSet<CommandObject>();
            objects.Add(new CommandObject(SystemObjectEnum.Server, context.CommandContext.Schema.Database.Server.ServerName, PermissionsEnum.ControlServer, false));
            objects.Add(new CommandObject(SystemObjectEnum.Database, context.CommandContext.Schema.Database.DatabaseName, PermissionsEnum.Control, false));
            objects.Add(new CommandObject(SystemObjectEnum.Schema, context.CommandContext.Schema.SchemaName, PermissionsEnum.Control, false));

            // la acción a validar en este punto debe ser diferente de "create" porque se quiere validar que los objetos del contexto del comando existan.
            this.Exist(context, objects);

            // se eliminan los objetos del contexto del comando para poder validar los objetos del comando con la acción correspondiente del comando.
            objects.Clear();
            */

            // aqui se valida con la acción del comando.
            this.Exist(context, context.CommandContext.Command.CommandObjects);

            return context;
        }

        /// <inheritdoc />
        public override void OnError(PipelineContext e)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Validate the existence of the object.
        /// </summary>
        /// <param name="context">Pipeline context.</param>
        /// <param name="objects">Objects to validate.</param>
        private void Exist(PipelineContext context, HashSet<CommandObject> objects)
        {
            foreach (CommandObject @object in objects)
            {
                Schema schema = @object.GetSchema(context.Kernel.Get<SpaceDbContext>(), context.SecurityContext.Login);
                bool exists = false;
                switch (@object.SecurableClass)
                {
                    case SystemObjectEnum.SourceColumn:
                        exists = this.ExistSourceColumn(@object.GranularObjectName, @object.Name, context.Kernel, schema);
                        break;
                    case SystemObjectEnum.Source:
                        // se valida si es una fuente del sistema Ej. para metadata.
                        SystemSourceEnum systemSource;
                        if (Enum.TryParse(@object.Name, true, out systemSource))
                        {
                            continue;
                        }
                        else
                        {
                            exists = this.ExistSource(@object.Name, context.Kernel, schema);
                        }

                        break;
                    case SystemObjectEnum.Stream:
                        exists = this.ExistStream(@object.Name, context.Kernel, schema);
                        break;
                    case SystemObjectEnum.View:
                        exists = this.ExistView(@object.Name, context.Kernel, schema);
                        break;
                    case SystemObjectEnum.Schema:
                        exists = this.ExistSchema(@object.Name, context.Kernel, schema);
                        break;
                    case SystemObjectEnum.DatabaseUser:
                        exists = this.ExistDatabaseUser(@object.Name, context.Kernel, schema);
                        break;
                    case SystemObjectEnum.DatabaseRole:
                        exists = this.ExistDatabaseRole(@object.Name, context.Kernel, schema);
                        break;
                    case SystemObjectEnum.Database:
                        exists = this.ExistDatabase(@object.Name, context.Kernel, schema);
                        break;
                    case SystemObjectEnum.Endpoint:
                        exists = this.ExistEndpoint(@object.Name, context.Kernel, schema);
                        break;
                    case SystemObjectEnum.Login:
                        exists = this.ExistLogin(@object.Name, context.Kernel, schema);
                        break;
                    case SystemObjectEnum.Server:
                        exists = this.ExistServer(@object.Name, context.Kernel);
                        break;
                    default:
                        throw new Exception(string.Format("Object type {0} not suported.", @object.SecurableClass));
                }

                if (exists && @object.IsNew)
                {
                    throw new Exception(string.Format("The {0} '{1}' already exist.", @object.SecurableClass, @object.Name));
                }
                else if (!exists && !@object.IsNew)
                {
                    throw new Exception(string.Format("The {0} '{1}' does not exist.", @object.SecurableClass, @object.Name));
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the entity exists.
        /// </summary>
        /// <param name="columnName">Name of the source column.</param>
        /// <param name="entityName">Entity name.</param>
        /// <param name="kernel">DI kernel.</param>
        /// <param name="schema">Execution schema.</param>
        /// <returns>Returns a value indicating whether the entity exists.</returns>
        private bool ExistSourceColumn(string columnName, string entityName, IKernel kernel, Schema schema)
        {
            SpaceDbContext context = kernel.Get<SpaceDbContext>();
            Source source = context.Sources.Single(x => x.SchemaId == schema.SchemaId && x.DatabaseId == schema.DatabaseId && x.ServerId == schema.ServerId && x.SourceName == entityName);
            bool exists = context.SourceColumns.Any(x => x.SchemaId == schema.SchemaId && x.DatabaseId == schema.DatabaseId && x.ServerId == schema.ServerId && x.SourceId == source.SourceId && x.ColumnName == columnName);
            return exists;
        }

        /// <summary>
        /// Gets a value indicating whether the entity exists.
        /// </summary>
        /// <param name="entityName">Entity name.</param>
        /// <param name="kernel">DI kernel.</param>
        /// <param name="schema">Execution schema.</param>
        /// <returns>Returns a value indicating whether the entity exists.</returns>
        private bool ExistSource(string entityName, IKernel kernel, Space.Database.Schema schema)
        {
            SpaceDbContext context = kernel.Get<SpaceDbContext>();
            bool exists = context.Sources.Any(x => x.SchemaId == schema.SchemaId && x.DatabaseId == schema.DatabaseId && x.ServerId == schema.ServerId && x.SourceName == entityName);
            return exists;
        }

        /// <summary>
        /// Gets a value indicating whether the entity exists.
        /// </summary>
        /// <param name="entityName">Entity name.</param>
        /// <param name="kernel">DI kernel.</param>
        /// <param name="schema">Execution schema.</param>
        /// <returns>Returns a value indicating whether the entity exists.</returns>
        private bool ExistStream(string entityName, IKernel kernel, Space.Database.Schema schema)
        {
            SpaceDbContext context = kernel.Get<SpaceDbContext>();
            bool exists = context.Streams.Any(x => x.SchemaId == schema.SchemaId && x.DatabaseId == schema.DatabaseId && x.ServerId == schema.ServerId && x.StreamName == entityName);
            return exists;
        }

        /// <summary>
        /// Gets a value indicating whether the entity exists.
        /// </summary>
        /// <param name="entityName">Entity name.</param>
        /// <param name="kernel">DI kernel.</param>
        /// <param name="schema">Execution schema.</param>
        /// <returns>Returns a value indicating whether the entity exists.</returns>
        private bool ExistView(string entityName, IKernel kernel, Space.Database.Schema schema)
        {
            SpaceDbContext context = kernel.Get<SpaceDbContext>();
            bool exists = context.Views.Any(x => x.SchemaId == schema.SchemaId && x.DatabaseId == schema.DatabaseId && x.ServerId == schema.ServerId && x.ViewName == entityName);
            return exists;
        }

        /// <summary>
        /// Gets a value indicating whether the entity exists.
        /// </summary>
        /// <param name="entityName">Entity name.</param>
        /// <param name="kernel">DI kernel.</param>
        /// <param name="schema">Execution schema.</param>
        /// <returns>Returns a value indicating whether the entity exists.</returns>
        private bool ExistSchema(string entityName, IKernel kernel, Space.Database.Schema schema)
        {
            SpaceDbContext context = kernel.Get<SpaceDbContext>();
            bool exists = context.Schemas.Any(x => x.DatabaseId == schema.DatabaseId && x.ServerId == schema.ServerId && x.SchemaName == entityName);
            return exists;
        }

        /// <summary>
        /// Gets a value indicating whether the entity exists.
        /// </summary>
        /// <param name="entityName">Entity name.</param>
        /// <param name="kernel">DI kernel.</param>
        /// <param name="schema">Execution schema.</param>
        /// <returns>Returns a value indicating whether the entity exists.</returns>
        private bool ExistDatabaseUser(string entityName, IKernel kernel, Space.Database.Schema schema)
        {
            SpaceDbContext context = kernel.Get<SpaceDbContext>();
            bool exists = context.DatabaseUsers.Any(x => x.DatabaseId == schema.DatabaseId && x.ServerId == schema.ServerId && x.DbUsrName == entityName);
            return exists;
        }

        /// <summary>
        /// Gets a value indicating whether the entity exists.
        /// </summary>
        /// <param name="entityName">Entity name.</param>
        /// <param name="kernel">DI kernel.</param>
        /// <param name="schema">Execution schema.</param>
        /// <returns>Returns a value indicating whether the entity exists.</returns>
        private bool ExistDatabaseRole(string entityName, IKernel kernel, Space.Database.Schema schema)
        {
            SpaceDbContext context = kernel.Get<SpaceDbContext>();
            bool exists = context.DatabaseRoles.Any(x => x.DatabaseId == schema.DatabaseId && x.ServerId == schema.ServerId && x.DbRoleName == entityName);
            return exists;
        }

        /// <summary>
        /// Gets a value indicating whether the entity exists.
        /// </summary>
        /// <param name="entityName">Entity name.</param>
        /// <param name="kernel">DI kernel.</param>
        /// <param name="schema">Execution schema.</param>
        /// <returns>Returns a value indicating whether the entity exists.</returns>
        private bool ExistDatabase(string entityName, IKernel kernel, Space.Database.Schema schema)
        {
            SpaceDbContext context = kernel.Get<SpaceDbContext>();
            bool exists = context.Databases.Any(x => x.ServerId == schema.ServerId && x.DatabaseName == entityName);
            return exists;
        }

        /// <summary>
        /// Gets a value indicating whether the entity exists.
        /// </summary>
        /// <param name="entityName">Entity name.</param>
        /// <param name="kernel">DI kernel.</param>
        /// <param name="schema">Execution schema.</param>
        /// <returns>Returns a value indicating whether the entity exists.</returns>
        private bool ExistEndpoint(string entityName, IKernel kernel, Space.Database.Schema schema)
        {
            SpaceDbContext context = kernel.Get<SpaceDbContext>();
            bool exists = context.Endpoints.Any(x => x.ServerId == schema.ServerId && x.EnpointName == entityName);
            return exists;
        }

        /// <summary>
        /// Gets a value indicating whether the entity exists.
        /// </summary>
        /// <param name="entityName">Entity name.</param>
        /// <param name="kernel">DI kernel.</param>
        /// <param name="schema">Execution schema.</param>
        /// <returns>Returns a value indicating whether the entity exists.</returns>
        private bool ExistLogin(string entityName, IKernel kernel, Space.Database.Schema schema)
        {
            SpaceDbContext context = kernel.Get<SpaceDbContext>();
            bool exists = context.Logins.Any(x => x.ServerId == schema.ServerId && x.LoginName == entityName);
            return exists;
        }

        /// <summary>
        /// Gets a value indicating whether the entity exists.
        /// </summary>
        /// <param name="entityName">Entity name.</param>
        /// <param name="kernel">DI kernel.</param>
        /// <returns>Returns a value indicating whether the entity exists.</returns>
        private bool ExistServer(string entityName, IKernel kernel)
        {
            SpaceDbContext context = kernel.Get<SpaceDbContext>();
            bool exists = context.Servers.Any(x => x.ServerName == entityName);
            return exists;
        }
    }
}
