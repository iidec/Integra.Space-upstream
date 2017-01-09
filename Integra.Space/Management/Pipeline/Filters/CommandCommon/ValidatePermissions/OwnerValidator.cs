//-----------------------------------------------------------------------
// <copyright file="OwnerValidator.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using System.Linq;
    using Integra.Space.Common;
    using Integra.Space.Database;
    using Ninject;

    /// <summary>
    /// Owner validator class.
    /// </summary>
    internal sealed class OwnerValidator
    {
        /// <summary>
        /// Login of the context.
        /// </summary>
        private Login login;

        /// <summary>
        /// User of the login.
        /// </summary>
        private DatabaseUser user;

        /// <summary>
        /// Schema execution.
        /// </summary>
        private Space.Database.Schema schema;
        
        /// <summary>
        /// Securable class.
        /// </summary>
        private SecurableClass securableClass;

        /// <summary>
        /// Database context.
        /// </summary>
        private SpaceDbContext databaseContext;

        /// <summary>
        /// Entity name if the command is not a create command.
        /// </summary>
        private string entityName;

        /// <summary>
        /// Initializes a new instance of the <see cref="OwnerValidator"/> class.
        /// </summary>
        /// <param name="login">Login of the context.</param>
        /// <param name="user">User of the login.</param>
        /// <param name="executionContext">Schema execution.</param>
        /// <param name="securableClass">Securable class.</param>
        /// <param name="databaseContext">Database context.</param>
        /// <param name="entityName">Entity name if the command is not a create command.</param>
        public OwnerValidator(Login login, DatabaseUser user, Space.Database.Schema executionContext, SecurableClass securableClass, SpaceDbContext databaseContext, string entityName)
        {
            this.login = login;
            this.user = user;
            this.schema = executionContext;
            this.securableClass = securableClass;
            this.databaseContext = databaseContext;
            this.entityName = entityName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OwnerValidator"/> class.
        /// </summary>
        /// <param name="login">Login of the context.</param>
        /// <param name="user">User of the login.</param>
        /// <param name="executionContext">Schema execution.</param>
        /// <param name="securableClass">Securable class.</param>
        /// <param name="databaseContext">Database context.</param>
        public OwnerValidator(Login login, DatabaseUser user, Space.Database.Schema executionContext, SecurableClass securableClass, SpaceDbContext databaseContext)
        {
            this.login = login;
            this.user = user;
            this.schema = executionContext;
            this.securableClass = securableClass;
            this.databaseContext = databaseContext;
        }

        /// <summary>
        /// Verify if the user or login is owner of an execution context object 
        /// </summary>
        /// <returns>A value indicating whether the user or login is owner.</returns>
        public bool IsOwner()
        {
            /*
            bool isCreate = false;
            if (this.granularPermission.GranularPermissionName.StartsWith("create", StringComparison.InvariantCultureIgnoreCase))
            {
                // esto se usa cuando el comando es create de tipos sin securable class padre. Ej: server.
                if (this.securableClass == null)
                {
                    return false;
                }

                isCreate = true;
            }
            */

            // se hace una validación en cadena para algunos objetos dependiendo el nivel jerarquico en que se encuentren.
            // si es para creación se envía al nivel superior mas proximo, de no tener nivel superior se salta a "default" para retornar false.
            SystemObjectEnum objectType;
            Enum.TryParse<SystemObjectEnum>(this.securableClass.SecurableName, true, out objectType);
            switch (objectType)
            {
                case SystemObjectEnum.Endpoint:
                    if (this.IsEndpointOwner(this.entityName, this.databaseContext, this.schema, this.login))
                    {
                        return true;
                    }

                    return false;
                case SystemObjectEnum.Database:
                    if (this.IsDatabaseOwner(this.entityName, this.databaseContext, this.schema, this.login))
                    {
                        return true;
                    }

                    return false;
                case SystemObjectEnum.DatabaseUser:
                    if (this.IsDatabaseOwner(this.schema.Database.DatabaseName, this.databaseContext, this.schema, this.login))
                    {
                        return true;
                    }

                    return false;
                case SystemObjectEnum.Schema:
                    if (this.IsSchemaOwner(this.entityName, this.databaseContext, this.schema, this.user))
                    {
                        return true;
                    }
                    else if (this.IsDatabaseOwner(this.schema.Database.DatabaseName, this.databaseContext, this.schema, this.login))
                    {
                        return true;
                    }

                    /*if (isCreate)
                    {
                        goto case SystemObjectEnum.Database;
                    }*/

                    return false;
                case SystemObjectEnum.DatabaseRole:
                    if (this.IsDatabaseRoleOwner(this.entityName, this.databaseContext, this.schema, this.user))
                    {
                        return true;
                    }
                    else if (this.IsDatabaseOwner(this.schema.Database.DatabaseName, this.databaseContext, this.schema, this.login))
                    {
                        return true;
                    }

                    /*if (isCreate)
                    {
                        goto case SystemObjectEnum.Database;
                    }*/

                    return false;
                case SystemObjectEnum.Source:
                    if (this.IsSourceOwner(this.entityName, this.databaseContext, this.schema, this.user))
                    {
                        return true;
                    }
                    else if (this.IsSchemaOwner(this.schema.SchemaName, this.databaseContext, this.schema, this.user))
                    {
                        return true;
                    }
                    else if (this.IsDatabaseOwner(this.schema.Database.DatabaseName, this.databaseContext, this.schema, this.login))
                    {
                        return true;
                    }

                    /*if (isCreate)
                    {
                        goto case SystemObjectEnum.Schema;
                    }*/

                    return false;
                case SystemObjectEnum.Stream:
                    if (this.IsStreamOwner(this.entityName, this.databaseContext, this.schema, this.user))
                    {
                        return true;
                    }
                    else if (this.IsSchemaOwner(this.schema.SchemaName, this.databaseContext, this.schema, this.user))
                    {
                        return true;
                    }
                    else if (this.IsDatabaseOwner(this.schema.Database.DatabaseName, this.databaseContext, this.schema, this.login))
                    {
                        return true;
                    }

                    /*if (isCreate)
                    {
                        goto case SystemObjectEnum.Schema;
                    }*/

                    return false;
                case SystemObjectEnum.View:
                    if (this.IsViewOwner(this.entityName, this.databaseContext, this.schema, this.user))
                    {
                        return true;
                    }
                    else if (this.IsSchemaOwner(this.schema.SchemaName, this.databaseContext, this.schema, this.user))
                    {
                        return true;
                    }
                    else if (this.IsDatabaseOwner(this.schema.Database.DatabaseName, this.databaseContext, this.schema, this.login))
                    {
                        return true;
                    }

                    /*if (isCreate)
                    {
                        goto case SystemObjectEnum.Schema;
                    }*/

                    return false;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the entity exists.
        /// </summary>
        /// <param name="entityName">Entity name.</param>
        /// <param name="context">Execution context.</param>
        /// <param name="schema">Execution schema.</param>
        /// <param name="user">Principal object.</param>
        /// <returns>Returns a value indicating whether the entity exists.</returns>
        private bool IsSourceOwner(string entityName, SpaceDbContext context, Space.Database.Schema schema, DatabaseUser user)
        {
            bool exists = context.Sources.Any(x => x.SchemaId == schema.SchemaId && x.DatabaseId == schema.DatabaseId && x.ServerId == schema.ServerId && x.SourceName == entityName && x.OwnerServerId == user.ServerId && x.OwnerDatabaseId == user.DatabaseId && x.OwnerId == user.DbUsrId);
            return exists;
        }

        /// <summary>
        /// Gets a value indicating whether the entity exists.
        /// </summary>
        /// <param name="entityName">Entity name.</param>
        /// <param name="context">Execution context.</param>
        /// <param name="schema">Execution schema.</param>
        /// <param name="user">Principal object.</param>
        /// <returns>Returns a value indicating whether the entity exists.</returns>
        private bool IsStreamOwner(string entityName, SpaceDbContext context, Space.Database.Schema schema, DatabaseUser user)
        {
            bool exists = context.Streams.Any(x => x.SchemaId == schema.SchemaId && x.DatabaseId == schema.DatabaseId && x.ServerId == schema.ServerId && x.StreamName == entityName && x.OwnerServerId == user.ServerId && x.OwnerDatabaseId == user.DatabaseId && x.OwnerId == user.DbUsrId);
            return exists;
        }

        /// <summary>
        /// Gets a value indicating whether the entity exists.
        /// </summary>
        /// <param name="entityName">Entity name.</param>
        /// <param name="context">Execution context.</param>
        /// <param name="schema">Execution schema.</param>
        /// <param name="user">Principal object.</param>
        /// <returns>Returns a value indicating whether the entity exists.</returns>
        private bool IsViewOwner(string entityName, SpaceDbContext context, Space.Database.Schema schema, DatabaseUser user)
        {
            bool exists = context.Views.Any(x => x.SchemaId == schema.SchemaId && x.DatabaseId == schema.DatabaseId && x.ServerId == schema.ServerId && x.ViewName == entityName && x.OwnerServerId == user.ServerId && x.OwnerDatabaseId == user.DatabaseId && x.OwnerId == user.DbUsrId);
            return exists;
        }

        /// <summary>
        /// Gets a value indicating whether the entity exists.
        /// </summary>
        /// <param name="entityName">Entity name.</param>
        /// <param name="context">Execution context.</param>
        /// <param name="schema">Execution schema.</param>
        /// <param name="user">Principal object.</param>
        /// <returns>Returns a value indicating whether the entity exists.</returns>
        private bool IsSchemaOwner(string entityName, SpaceDbContext context, Space.Database.Schema schema, DatabaseUser user)
        {
            bool exists = context.Schemas.Any(x => x.DatabaseId == schema.DatabaseId && x.ServerId == schema.ServerId && x.SchemaName == entityName && x.OwnerServerId == user.ServerId && x.OwnerDatabaseId == user.DatabaseId && x.OwnerId == user.DbUsrId);
            return exists;
        }

        /// <summary>
        /// Gets a value indicating whether the entity exists.
        /// </summary>
        /// <param name="entityName">Entity name.</param>
        /// <param name="context">Execution context.</param>
        /// <param name="schema">Execution schema.</param>
        /// <param name="user">Principal object.</param>
        /// <returns>Returns a value indicating whether the entity exists.</returns>
        private bool IsDatabaseRoleOwner(string entityName, SpaceDbContext context, Space.Database.Schema schema, DatabaseUser user)
        {
            bool exists = context.DatabaseRoles.Any(x => x.DatabaseId == schema.DatabaseId && x.ServerId == schema.ServerId && x.DbRoleName == entityName && x.OwnerServerId == user.ServerId && x.OwnerDatabaseId == user.DatabaseId && x.OwnerId == user.DbUsrId);
            return exists;
        }

        /// <summary>
        /// Gets a value indicating whether the entity exists.
        /// </summary>
        /// <param name="entityName">Entity name.</param>
        /// <param name="context">Execution context.</param>
        /// <param name="schema">Execution schema.</param>
        /// <param name="login">Principal object.</param>
        /// <returns>Returns a value indicating whether the entity exists.</returns>
        private bool IsDatabaseOwner(string entityName, SpaceDbContext context, Space.Database.Schema schema, Login login)
        {
            bool exists = context.Databases.Any(x => x.ServerId == schema.ServerId && x.DatabaseName == entityName && x.OwnerServerId == login.ServerId && x.OwnerId == login.LoginId);
            return exists;
        }

        /// <summary>
        /// Gets a value indicating whether the entity exists.
        /// </summary>
        /// <param name="entityName">Entity name.</param>
        /// <param name="context">Execution context.</param>
        /// <param name="schema">Execution schema.</param>
        /// <param name="login">Principal object.</param>
        /// <returns>Returns a value indicating whether the entity exists.</returns>
        private bool IsEndpointOwner(string entityName, SpaceDbContext context, Space.Database.Schema schema, Login login)
        {
            bool exists = context.Endpoints.Any(x => x.ServerId == schema.ServerId && x.EnpointName == entityName && x.OwnerServerId == login.ServerId && x.OwnerId == login.LoginId);
            return exists;
        }
    }
}
