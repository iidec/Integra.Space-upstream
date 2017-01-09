//-----------------------------------------------------------------------
// <copyright file="CommandObjectExtensions.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline
{
    using System.Linq;
    using Database;

    /// <summary>
    /// Command object extensions.
    /// </summary>
    internal static class CommandObjectExtensions
    {
        /// <summary>
        /// Gets the schema to which the object belongs.
        /// </summary>
        /// <param name="commandObject">Command object.</param>
        /// <param name="context">Space database context.</param>
        /// <param name="login">Login of the user.</param>
        /// <param name="database">The database to which the object belongs.</param>
        /// <returns>Schema of the command object.</returns>
        public static Space.Database.Schema GetSchema(this Language.CommandObject commandObject, SpaceDbContext context, Login login, Database database)
        {
            string schemaName = commandObject.SchemaName;

            Space.Database.Schema schemaAux = null;
            if (schemaName != null)
            {
                if (context.Schemas.Select(x => x.SchemaName).Contains(schemaName))
                {
                    schemaAux = context.Schemas.Single(x => x.SchemaName == schemaName);
                }
                else
                {
                    throw new System.Exception(string.Format("The schema does not exist in the database: '{0}'.", database.DatabaseName));
                }
            }
            else
            {
                DatabaseUser userAux = null;

                try
                {
                    // obtengo el usuario que quiere ejecutar el comando. Este tambien se calcula en la clase SecureContextBuilderFilter
                    userAux = login.DatabaseUsers.Single(x => x.ServerId == database.ServerId && x.DatabaseId == database.DatabaseId);
                }
                catch (System.Exception e)
                {
                    throw new System.Exception(string.Format("No user mapped at the database '{0}' for the login '{1}'.", database.DatabaseName, login.LoginName));
                }

                // obtengo el esquema de ejecución.
                schemaAux = context.DatabaseUsers.Single(x => x.DatabaseId == database.DatabaseId && x.ServerId == database.ServerId && x.DbUsrName == userAux.DbUsrName).DefaultSchema;
            }

            return schemaAux;
        }

        /// <summary>
        /// Gets the schema to which the object belongs.
        /// </summary>
        /// <param name="commandObject">Command object.</param>
        /// <param name="context">Space database context.</param>
        /// <param name="login">Login of the user.</param>
        /// <returns>Schema of the command object.</returns>
        public static Space.Database.Schema GetSchema(this Language.CommandObject commandObject, SpaceDbContext context, Login login)
        {
            Database database = GetDatabase(commandObject, context, login);
            return GetSchema(commandObject, context, login, database);
        }

        /// <summary>
        /// Gets the database to which the object belongs.
        /// </summary>
        /// <param name="commandObject">Command object.</param>
        /// <param name="context">Space database context.</param>
        /// <param name="login">Login of the user.</param>
        /// <returns>The database of the command object.</returns>
        public static Database GetDatabase(this Language.CommandObject commandObject, SpaceDbContext context, Login login)
        {
            string databaseName = commandObject.DatabaseName;
            Database databaseAux = null;
            if (databaseName != null)
            {
                databaseAux = context.Databases.Single(x => x.ServerId == login.ServerId && x.DatabaseName == databaseName);
            }
            else
            {
                databaseAux = context.Databases.Single(x => x.ServerId == login.DefaultDatabaseServerId && x.DatabaseId == login.DefaultDatabaseId);
            }

            return databaseAux;
        }

        /// <summary>
        /// Gets the user for the specified database and login.
        /// </summary>
        /// <param name="commandObject">Command object.</param>
        /// <param name="context">Space database context.</param>
        /// <param name="login">Login of the user.</param>
        /// <returns>The database user for the specified database and login.</returns>
        public static DatabaseUser GetUser(this Language.CommandObject commandObject, SpaceDbContext context, Login login)
        {
            Database db = GetDatabase(commandObject, context, login);
            return login.DatabaseUsers.Where(x => x.DatabaseId == db.DatabaseId && x.ServerId == db.ServerId).Single();
        }
    }
}
