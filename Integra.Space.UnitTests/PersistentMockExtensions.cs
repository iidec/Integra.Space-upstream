using Integra.Space.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integra.Space.UnitTests
{
    public static class PersistentMockExtensions
    {
        public static Server AddDatabase(this Server server, Database.Database database, Login owner)
        {
            if (server == null) throw new ArgumentException("Server is null.");
            if (database == null) throw new ArgumentException("Database is null");
            if (owner.Server != null && server.ServerId != owner.ServerId) throw new ArgumentException("Invalid owner");

            // especifico que la base de datos pertenece al servidor.
            server.Databases.Add(database);
            database.Server = server;
            database.ServerId = server.ServerId;

            // especifico el owner de la base de datos.
            database.Login = owner;
            database.Logins.Add(owner);
            database.OwnerId = owner.LoginId;
            database.OwnerServerId = owner.ServerId;

            foreach (var login in database.Logins)
            {
                if (login.ServerId == database.ServerId && login.DefaultDatabaseId == database.DatabaseId)
                {
                    login.DefaultDatabaseServerId = database.ServerId;
                }
            }

            // se le especifica el servidor a la base de datos por defecto del owner de esta base de datos si la base de datos por defecto del owner es esta
            if (owner.ServerId == database.ServerId && owner.DefaultDatabaseId == database.DatabaseId)
            {
                owner.DefaultDatabaseServerId = database.ServerId;
            }

            return server;
        }

        public static Server AddLogin(this Server server, Login login, Database.Database defaultDatabase, ServerRole serverRole)
        {
            if (server == null) throw new ArgumentException("Server is null.");
            if (login == null) throw new ArgumentException("login is null");
            if (defaultDatabase.Server != null && defaultDatabase.ServerId != server.ServerId) throw new ArgumentException("Invalid database.");

            // especifico que el login pertenece al server.
            server.Logins.Add(login);
            login.Server = server;
            login.ServerId = server.ServerId;

            // especifico la base de datos por defecto.
            defaultDatabase.Logins.Add(login);
            login.Database = defaultDatabase;
            login.Databases.Add(defaultDatabase);
            login.DefaultDatabaseId = defaultDatabase.DatabaseId;
            login.DefaultDatabaseServerId = defaultDatabase.ServerId;

            foreach (var db in login.Databases)
            {
                if (db.ServerId == login.ServerId && db.OwnerId == login.LoginId)
                {
                    db.OwnerServerId = login.ServerId;
                }
            }

            // se especifica el servidor del propietario de la base de datos por defecto si el propietario es este login
            if (defaultDatabase.ServerId == login.ServerId && defaultDatabase.OwnerId == login.LoginId)
            {
                defaultDatabase.OwnerServerId = login.ServerId;
            }

            // le especifico el server rol
            if (serverRole != null)
            {
                serverRole.Logins.Add(login);
                login.ServerRoles.Add(serverRole);
            }

            return server;
        }

        public static Server AddServerRole(this Server server, ServerRole serverRole)
        {
            if (server == null) throw new ArgumentException("Server is null.");
            if (serverRole == null) throw new ArgumentException("Server role is null");

            // se agrega el role de seridor al servidor
            server.ServerRoles.Add(serverRole);
            serverRole.Server = server;
            serverRole.ServerId = server.ServerId;

            return server;
        }

        public static Database.Database AddSchema(this Database.Database database, Schema schema, DatabaseUser owner)
        {
            if (database == null) throw new ArgumentException("Database is null");
            if (schema == null) throw new ArgumentException("Role is null");
            if (owner.Database != null && database.ServerId != owner.ServerId) throw new ArgumentException("Invalid owner.");

            // se agrega el role a la base de datos
            database.Schemas.Add(schema);
            schema.Database = database;
            schema.ServerId = database.ServerId;
            schema.DatabaseId = database.DatabaseId;

            // especifico el owner
            owner.Schemas.Add(schema);
            schema.DatabaseUser = owner;
            schema.OwnerServerId = owner.ServerId;
            schema.OwnerDatabaseId = owner.DatabaseId;
            schema.OwnerId = owner.DbUsrId;

            foreach (var user in schema.DatabaseUsers)
            {
                if (user.DatabaseId == database.DatabaseId && user.DefaultSchemaId == schema.SchemaId)
                {
                    owner.DefaultSchemaServerId = schema.ServerId;
                    owner.DefaultSchemaDatabaseId = schema.DatabaseId;
                }
            }

            // agrego el servidor y base de datos al propietario del de este esquema si el default esquema de este propietario es este esquema
            if (owner.DatabaseId == database.DatabaseId && owner.DefaultSchemaId == schema.SchemaId)
            {
                owner.DefaultSchemaServerId = schema.ServerId;
                owner.DefaultSchemaDatabaseId = schema.DatabaseId;
            }

            return database;
        }

        public static Database.Database AddUser(this Database.Database database, DatabaseUser user, Login login, Schema defaultSchema)
        {
            if (database == null) throw new ArgumentException("Database is null");
            if (user == null) throw new ArgumentException("User is null");
            if (login == null) throw new ArgumentException("Login is null.");
            if (database.ServerId != login.ServerId) throw new ArgumentException("Invalid login.");
            if (defaultSchema.Database != null && database.ServerId != defaultSchema.ServerId && database.DatabaseId != defaultSchema.DatabaseId) throw new ArgumentException("Invalid default schema.");

            // se agrega el usuario a la base de datos.
            database.DatabaseUsers.Add(user);
            user.Database = database;
            user.DatabaseId = database.DatabaseId;
            user.ServerId = database.ServerId;

            // se mapea el login.
            login.DatabaseUsers.Add(user);
            user.Login = login;
            user.LoginId = login.ServerId;
            user.LoginServerId = login.LoginId;

            // especifico el esquema por defecto.
            defaultSchema.DatabaseUsers.Add(user);
            user.DefaultSchema = defaultSchema;
            user.Schemas.Add(defaultSchema);
            user.DefaultSchemaServerId = defaultSchema.ServerId;
            user.DefaultSchemaDatabaseId = defaultSchema.DatabaseId;
            user.DefaultSchemaId = defaultSchema.SchemaId;

            foreach(var schema in user.Schemas)
            {
                if (schema.ServerId == user.ServerId && schema.DatabaseId == user.DatabaseId && schema.OwnerId == user.DbUsrId)
                {
                    schema.OwnerServerId = user.ServerId;
                    schema.OwnerDatabaseId = user.DatabaseId;
                }
            }

            // agrego el servidor y base de datos del propietario del esquema por defecto de este usuario si este usuario es el propietario
            if (defaultSchema.ServerId == user.ServerId && defaultSchema.DatabaseId == user.DatabaseId && defaultSchema.OwnerId == user.DbUsrId)
            {
                defaultSchema.OwnerServerId = user.ServerId;
                defaultSchema.OwnerDatabaseId = user.DatabaseId;
            }

            return database;
        }

        public static Database.Database AddRole(this Database.Database database, DatabaseRole role, DatabaseUser owner)
        {
            if (database == null) throw new ArgumentException("Database is null");
            if (role == null) throw new ArgumentException("Role is null");
            if (owner.Database != null && database.ServerId != owner.ServerId) throw new ArgumentException("Invalid owner.");

            // se agrega el role a la base de datos
            database.DatabaseRoles.Add(role);
            role.Database = database;
            role.ServerId = database.ServerId;
            role.DatabaseId = database.DatabaseId;

            // especifico el owner
            owner.DatabaseRoles.Add(role);
            role.DatabaseUser = owner;
            role.OwnerServerId = owner.ServerId;
            role.OwnerDatabaseId = owner.DatabaseId;
            role.OwnerId = owner.DbUsrId;

            return database;
        }

        public static Schema AddStream(this Schema schema, Stream stream, DatabaseUser owner, params Source[] sources)
        {
            if (schema == null) throw new ArgumentException("Schema is null");
            if (stream == null) throw new ArgumentException("Stream is null");
            if (owner.ServerId != owner.ServerId) throw new ArgumentException("Invalid owner.");

            // agrego el stream al esquema
            schema.Streams.Add(stream);
            stream.Schema = schema;
            stream.ServerId = schema.ServerId;
            stream.DatabaseId = schema.DatabaseId;
            stream.SchemaId = schema.SchemaId;

            // especifico el owner
            owner.Streams.Add(stream);
            stream.DatabaseUser = owner;
            stream.OwnerServerId = owner.ServerId;
            stream.OwnerDatabaseId = owner.DatabaseId;
            stream.OwnerId = owner.DbUsrId;

            // se referencian alas fuentes
            foreach (Source source in sources)
            {
                SourceByStream sbs = new SourceByStream()
                {
                    RelationshipId = Guid.NewGuid(),
                    Stream = stream,
                    StreamServerId = stream.ServerId,
                    StreamDatabaseId = stream.DatabaseId,
                    StreamSchemaId = stream.SchemaId,
                    StreamId = stream.StreamId,
                    Source = source,
                    SourceServerId = source.ServerId,
                    SourceDatabaseId = source.DatabaseId,
                    SourceSchemaId = source.SchemaId,
                    SourceId = source.SourceId,
                    IsInputSource = source.SourceName.StartsWith("input", StringComparison.InvariantCultureIgnoreCase) ? true : false
                };

                source.Streams.Add(sbs);
                stream.Sources.Add(sbs);
            }

            return schema;
        }

        public static Stream AddStreamColumn(this Stream stream, StreamColumn column)
        {
            if (stream == null) throw new ArgumentException("Stream is null");
            if (column == null) throw new ArgumentException("Stream column is null");

            stream.ProjectionColumns.Add(column);
            column.Stream = stream;
            column.ServerId = stream.ServerId;
            column.DatabaseId = stream.DatabaseId;
            column.SchemaId = stream.SchemaId;
            column.StreamId = stream.StreamId;

            return stream;
        }

        public static Stream AddStreamColumn(this Stream stream, params StreamColumn[] columns)
        {
            if (stream == null) throw new ArgumentException("Stream is null");
            if (columns == null) throw new ArgumentException("Stream columns are null");

            foreach (StreamColumn column in columns)
            {
                stream.ProjectionColumns.Add(column);
                column.Stream = stream;
                column.ServerId = stream.ServerId;
                column.DatabaseId = stream.DatabaseId;
                column.SchemaId = stream.SchemaId;
                column.StreamId = stream.StreamId;
            }

            return stream;
        }

        public static Schema AddSource(this Schema schema, Source source, DatabaseUser owner)
        {
            if (schema == null) throw new ArgumentException("Schema is null");
            if (source == null) throw new ArgumentException("Source is null");
            if (owner.ServerId != owner.ServerId) throw new ArgumentException("Invalid owner.");

            // agrego el stream al esquema
            schema.Sources.Add(source);
            source.Schema = schema;
            source.ServerId = schema.ServerId;
            source.DatabaseId = schema.DatabaseId;
            source.SchemaId = schema.SchemaId;

            // especifico el owner
            owner.Sources.Add(source);
            source.DatabaseUser = owner;
            source.OwnerServerId = owner.ServerId;
            source.OwnerDatabaseId = owner.DatabaseId;
            source.OwnerId = owner.DbUsrId;

            return schema;
        }

        public static Source AddSourceColumn(this Source source, SourceColumn column)
        {
            if (source == null) throw new ArgumentException("Source is null");
            if (column == null) throw new ArgumentException("Source column is null");

            byte lastIndex = 1;
            if (source.Columns != null && source.Columns.Count != 0)
            {
                lastIndex = source.Columns.Max(x => x.ColumnIndex);
            }

            column.ColumnIndex = ++lastIndex;
            source.Columns.Add(column);

            column.Source = source;
            column.ServerId = source.ServerId;
            column.DatabaseId = source.DatabaseId;
            column.SchemaId = source.SchemaId;
            column.SourceId = source.SourceId;

            return source;
        }

        public static Source AddSourceColumn(this Source source, params SourceColumn[] columns)
        {
            if (source == null) throw new ArgumentException("Source is null");
            if (columns == null) throw new ArgumentException("Source columns are null");

            byte lastIndex = 1;
            if (source.Columns != null && source.Columns.Count != 0)
            {
                lastIndex = source.Columns.Max(x => x.ColumnIndex);
            }

            foreach (SourceColumn column in columns)
            {
                column.ColumnIndex = ++lastIndex;
                source.Columns.Add(column);
                column.Source = source;
                column.ServerId = source.ServerId;
                column.DatabaseId = source.DatabaseId;
                column.SchemaId = source.SchemaId;
                column.SourceId = source.SourceId;
            }

            return source;
        }

        public static Database.Database AddDatabasePermissionToUser(this Database.Database securable, DatabaseAssignedPermissionsToUser permission, DatabaseUser principal, List<PermissionView> view)
        {
            if (securable == null) throw new ArgumentException("Database is null");
            if (permission == null) throw new ArgumentException("Permission are null");
            if (principal == null) throw new ArgumentException("Principal are null");
            if (view == null) throw new ArgumentException("Permission view is null");

            securable.DatabaseAssignedPermissionsToUsers.Add(permission);

            view.Add(new PermissionView()
            {
                ServerIdOfPrincipal = principal.ServerId,
                DatabaseIdOfPrincipal = principal.DatabaseId,
                PrincipalId = principal.DbUsrId,

                ServerIdOfSecurable = securable.ServerId,
                DatabaseIdOfSecurable = null,
                SchemaIdOfSecurable = null,
                SecurableId = securable.DatabaseId,

                GranularPermissionId = permission.GranularPermissionId,
                SecurableClassId = permission.SecurableClassId,

                Denied = permission.Denied,
                Granted = permission.Granted,
                WithGrantOption = permission.WithGrantOption
            });

            return securable;
        }

        public static Database.Database AddDatabasePermissionToDbRole(this Database.Database securable, DatabaseAssignedPermissionsToDBRole permission, DatabaseRole principal, List<PermissionView> view)
        {
            if (securable == null) throw new ArgumentException("Database is null");
            if (permission == null) throw new ArgumentException("Permission are null");
            if (principal == null) throw new ArgumentException("Principal are null");
            if (view == null) throw new ArgumentException("Permission view is null");

            securable.DatabaseAssignedPermissionsToDBRoles.Add(permission);

            view.Add(new PermissionView()
            {
                ServerIdOfPrincipal = principal.ServerId,
                DatabaseIdOfPrincipal = principal.DatabaseId,
                PrincipalId = principal.DbRoleId,

                ServerIdOfSecurable = securable.ServerId,
                DatabaseIdOfSecurable = null,
                SchemaIdOfSecurable = null,
                SecurableId = securable.DatabaseId,

                GranularPermissionId = permission.GranularPermissionId,
                SecurableClassId = permission.SecurableClassId,

                Denied = permission.Denied,
                Granted = permission.Granted,
                WithGrantOption = permission.WithGrantOption
            });

            return securable;
        }

        public static DatabaseRole AddDbRolePermissionToDbRole(this DatabaseRole securable, DBRoleAssignedPermissionsToDBRole permission, DatabaseRole principal, List<PermissionView> view)
        {
            if (securable == null) throw new ArgumentException("Database is null");
            if (permission == null) throw new ArgumentException("Permission are null");
            if (principal == null) throw new ArgumentException("Principal are null");
            if (view == null) throw new ArgumentException("Permission view is null");

            securable.DBRolesAssignedPermissionsToDBRoles.Add(permission);

            view.Add(new PermissionView()
            {
                ServerIdOfPrincipal = principal.ServerId,
                DatabaseIdOfPrincipal = principal.DatabaseId,
                PrincipalId = principal.DbRoleId,

                ServerIdOfSecurable = securable.ServerId,
                DatabaseIdOfSecurable = securable.DatabaseId,
                SchemaIdOfSecurable = null,
                SecurableId = securable.DbRoleId,

                GranularPermissionId = permission.GranularPermissionId,
                SecurableClassId = permission.SecurableClassId,

                Denied = permission.Denied,
                Granted = permission.Granted,
                WithGrantOption = permission.WithGrantOption
            });

            return securable;
        }

        public static DatabaseRole AddDbRolePermissionToUser(this DatabaseRole securable, DBRoleAssignedPermissionsToUser permission, DatabaseUser principal, List<PermissionView> view)
        {
            if (securable == null) throw new ArgumentException("Database is null");
            if (permission == null) throw new ArgumentException("Permission are null");
            if (principal == null) throw new ArgumentException("Principal are null");
            if (view == null) throw new ArgumentException("Permission view is null");

            securable.DBRolesAssignedPermissionsToUsers.Add(permission);

            view.Add(new PermissionView()
            {
                ServerIdOfPrincipal = principal.ServerId,
                DatabaseIdOfPrincipal = principal.DatabaseId,
                PrincipalId = principal.DbUsrId,

                ServerIdOfSecurable = securable.ServerId,
                DatabaseIdOfSecurable = securable.DatabaseId,
                SchemaIdOfSecurable = null,
                SecurableId = securable.DbRoleId,

                GranularPermissionId = permission.GranularPermissionId,
                SecurableClassId = permission.SecurableClassId,

                Denied = permission.Denied,
                Granted = permission.Granted,
                WithGrantOption = permission.WithGrantOption
            });

            return securable;
        }

        public static Login AddLoginPermissionToLogin(this Login securable, LoginAssignedPermissionsToLogin permission, Login principal, List<PermissionView> view)
        {
            if (securable == null) throw new ArgumentException("Database is null");
            if (permission == null) throw new ArgumentException("Permission are null");
            if (principal == null) throw new ArgumentException("Principal are null");
            if (view == null) throw new ArgumentException("Permission view is null");

            securable.LoginsAssignedPermissionsToLogins.Add(permission);

            view.Add(new PermissionView()
            {
                ServerIdOfPrincipal = principal.ServerId,
                DatabaseIdOfPrincipal = null,
                PrincipalId = principal.LoginId,

                ServerIdOfSecurable = securable.ServerId,
                DatabaseIdOfSecurable = null,
                SchemaIdOfSecurable = null,
                SecurableId = securable.LoginId,

                GranularPermissionId = permission.GranularPermissionId,
                SecurableClassId = permission.SecurableClassId,

                Denied = permission.Denied,
                Granted = permission.Granted,
                WithGrantOption = permission.WithGrantOption
            });

            return securable;
        }

        public static Login AddLoginPermissionToServerRole(this Login securable, LoginAssignedPermissionsToServerRole permission, ServerRole principal, List<PermissionView> view)
        {
            if (securable == null) throw new ArgumentException("Database is null");
            if (permission == null) throw new ArgumentException("Permission are null");
            if (principal == null) throw new ArgumentException("Principal are null");
            if (view == null) throw new ArgumentException("Permission view is null");

            securable.LoginsAssignedPermissionsToServerRoles.Add(permission);

            view.Add(new PermissionView()
            {
                ServerIdOfPrincipal = principal.ServerId,
                DatabaseIdOfPrincipal = null,
                PrincipalId = principal.ServerRoleId,

                ServerIdOfSecurable = securable.ServerId,
                DatabaseIdOfSecurable = null,
                SchemaIdOfSecurable = null,
                SecurableId = securable.LoginId,

                GranularPermissionId = permission.GranularPermissionId,
                SecurableClassId = permission.SecurableClassId,

                Denied = permission.Denied,
                Granted = permission.Granted,
                WithGrantOption = permission.WithGrantOption
            });

            return securable;
        }

        public static Schema AddSchemaPermissionToUser(this Schema securable, SchemaAssignedPermissionsToUser permission, DatabaseUser principal, List<PermissionView> view)
        {
            if (securable == null) throw new ArgumentException("Database is null");
            if (permission == null) throw new ArgumentException("Permission are null");
            if (principal == null) throw new ArgumentException("Principal are null");
            if (view == null) throw new ArgumentException("Permission view is null");

            securable.SchemaAssignedPermissionsToUsers.Add(permission);

            view.Add(new PermissionView()
            {
                ServerIdOfPrincipal = principal.ServerId,
                DatabaseIdOfPrincipal = principal.DatabaseId,
                PrincipalId = principal.DbUsrId,

                ServerIdOfSecurable = securable.ServerId,
                DatabaseIdOfSecurable = securable.DatabaseId,
                SchemaIdOfSecurable = null,
                SecurableId = securable.SchemaId,

                GranularPermissionId = permission.GranularPermissionId,
                SecurableClassId = permission.SecurableClassId,

                Denied = permission.Denied,
                Granted = permission.Granted,
                WithGrantOption = permission.WithGrantOption
            });

            return securable;
        }

        public static Schema AddSchemaPermissionToDbRole(this Schema securable, SchemaAssignedPermissionsToDBRole permission, DatabaseRole principal, List<PermissionView> view)
        {
            if (securable == null) throw new ArgumentException("Database is null");
            if (permission == null) throw new ArgumentException("Permission are null");
            if (principal == null) throw new ArgumentException("Principal are null");
            if (view == null) throw new ArgumentException("Permission view is null");

            securable.SchemaAssignedPermissionsToDBRoles.Add(permission);

            view.Add(new PermissionView()
            {
                ServerIdOfPrincipal = principal.ServerId,
                DatabaseIdOfPrincipal = principal.DatabaseId,
                PrincipalId = principal.DbRoleId,

                ServerIdOfSecurable = securable.ServerId,
                DatabaseIdOfSecurable = securable.DatabaseId,
                SchemaIdOfSecurable = null,
                SecurableId = securable.SchemaId,

                GranularPermissionId = permission.GranularPermissionId,
                SecurableClassId = permission.SecurableClassId,

                Denied = permission.Denied,
                Granted = permission.Granted,
                WithGrantOption = permission.WithGrantOption
            });

            return securable;
        }

        public static DatabaseUser AddUserPermissionToDbRole(this DatabaseUser securable, UserAssignedPermissionsToDBRole permission, DatabaseRole principal, List<PermissionView> view)
        {
            if (securable == null) throw new ArgumentException("Database is null");
            if (permission == null) throw new ArgumentException("Permission are null");
            if (principal == null) throw new ArgumentException("Principal are null");
            if (view == null) throw new ArgumentException("Permission view is null");

            securable.UserAssignedPermissionsToDBRoles.Add(permission);

            view.Add(new PermissionView()
            {
                ServerIdOfPrincipal = principal.ServerId,
                DatabaseIdOfPrincipal = principal.DatabaseId,
                PrincipalId = principal.DbRoleId,

                ServerIdOfSecurable = securable.ServerId,
                DatabaseIdOfSecurable = securable.DatabaseId,
                SchemaIdOfSecurable = null,
                SecurableId = securable.DbUsrId,

                GranularPermissionId = permission.GranularPermissionId,
                SecurableClassId = permission.SecurableClassId,

                Denied = permission.Denied,
                Granted = permission.Granted,
                WithGrantOption = permission.WithGrantOption
            });

            return securable;
        }

        public static DatabaseUser AddUserPermissionToUser(this DatabaseUser securable, UserAssignedPermissionsToUsers permission, DatabaseUser principal, List<PermissionView> view)
        {
            if (securable == null) throw new ArgumentException("Database is null");
            if (permission == null) throw new ArgumentException("Permission are null");
            if (principal == null) throw new ArgumentException("Principal are null");
            if (view == null) throw new ArgumentException("Permission view is null");

            securable.UserAssignedPermissionsToUsers.Add(permission);

            view.Add(new PermissionView()
            {
                ServerIdOfPrincipal = principal.ServerId,
                DatabaseIdOfPrincipal = principal.DatabaseId,
                PrincipalId = principal.DbUsrId,

                ServerIdOfSecurable = securable.ServerId,
                DatabaseIdOfSecurable = securable.DatabaseId,
                SchemaIdOfSecurable = null,
                SecurableId = securable.DbUsrId,

                GranularPermissionId = permission.GranularPermissionId,
                SecurableClassId = permission.SecurableClassId,

                Denied = permission.Denied,
                Granted = permission.Granted,
                WithGrantOption = permission.WithGrantOption
            });

            return securable;
        }

        public static Server AddServerPermissionToLogin(this Server securable, ServerAssignedPermissionsToLogin permission, Login principal, List<PermissionView> view)
        {
            if (securable == null) throw new ArgumentException("Database is null");
            if (permission == null) throw new ArgumentException("Permission are null");
            if (principal == null) throw new ArgumentException("Principal are null");
            if (view == null) throw new ArgumentException("Permission view is null");

            securable.ServersAssignedPermissionsToLogins.Add(permission);

            view.Add(new PermissionView()
            {
                ServerIdOfPrincipal = principal.ServerId,
                DatabaseIdOfPrincipal = null,
                PrincipalId = principal.LoginId,

                ServerIdOfSecurable = null,
                DatabaseIdOfSecurable = null,
                SchemaIdOfSecurable = null,
                SecurableId = securable.ServerId,

                GranularPermissionId = permission.GranularPermissionId,
                SecurableClassId = permission.SecurableClassId,

                Denied = permission.Denied,
                Granted = permission.Granted,
                WithGrantOption = permission.WithGrantOption
            });

            return securable;
        }

        public static Server AddServerPermissionToServerRole(this Server securable, ServerAssignedPermissionsToServerRole permission, ServerRole principal, List<PermissionView> view)
        {
            if (securable == null) throw new ArgumentException("Database is null");
            if (permission == null) throw new ArgumentException("Permission are null");
            if (principal == null) throw new ArgumentException("Principal are null");
            if (view == null) throw new ArgumentException("Permission view is null");

            securable.ServersAssignedPermissionsToServerRoles.Add(permission);

            view.Add(new PermissionView()
            {
                ServerIdOfPrincipal = principal.ServerId,
                DatabaseIdOfPrincipal = null,
                PrincipalId = principal.ServerRoleId,

                ServerIdOfSecurable = null,
                DatabaseIdOfSecurable = null,
                SchemaIdOfSecurable = null,
                SecurableId = securable.ServerId,

                GranularPermissionId = permission.GranularPermissionId,
                SecurableClassId = permission.SecurableClassId,

                Denied = permission.Denied,
                Granted = permission.Granted,
                WithGrantOption = permission.WithGrantOption
            });

            return securable;
        }

        public static Source AddSourcePermissionToUser(this Source securable, SourceAssignedPermissionsToUser permission, DatabaseUser principal, List<PermissionView> view)
        {
            if (securable == null) throw new ArgumentException("Database is null");
            if (permission == null) throw new ArgumentException("Permission are null");
            if (principal == null) throw new ArgumentException("Principal are null");
            if (view == null) throw new ArgumentException("Permission view is null");

            securable.SourceAssignedPermissionsToUsers.Add(permission);

            view.Add(new PermissionView()
            {
                ServerIdOfPrincipal = principal.ServerId,
                DatabaseIdOfPrincipal = principal.DatabaseId,
                PrincipalId = principal.DbUsrId,

                ServerIdOfSecurable = securable.ServerId,
                DatabaseIdOfSecurable = securable.DatabaseId,
                SchemaIdOfSecurable = securable.SchemaId,
                SecurableId = securable.SourceId,

                GranularPermissionId = permission.GranularPermissionId,
                SecurableClassId = permission.SecurableClassId,

                Denied = permission.Denied,
                Granted = permission.Granted,
                WithGrantOption = permission.WithGrantOption
            });

            return securable;
        }

        public static Source AddSourcePermissionToDbRole(this Source securable, SourceAssignedPermissionsToDBRole permission, DatabaseRole principal, List<PermissionView> view)
        {
            if (securable == null) throw new ArgumentException("Database is null");
            if (permission == null) throw new ArgumentException("Permission are null");
            if (principal == null) throw new ArgumentException("Principal are null");
            if (view == null) throw new ArgumentException("Permission view is null");

            securable.SourceAssignedPermissionsToDBRoles.Add(permission);

            view.Add(new PermissionView()
            {
                ServerIdOfPrincipal = principal.ServerId,
                DatabaseIdOfPrincipal = principal.DatabaseId,
                PrincipalId = principal.DbRoleId,

                ServerIdOfSecurable = securable.ServerId,
                DatabaseIdOfSecurable = securable.DatabaseId,
                SchemaIdOfSecurable = securable.SchemaId,
                SecurableId = securable.SourceId,

                GranularPermissionId = permission.GranularPermissionId,
                SecurableClassId = permission.SecurableClassId,

                Denied = permission.Denied,
                Granted = permission.Granted,
                WithGrantOption = permission.WithGrantOption
            });

            return securable;
        }

        public static Stream AddStreamPermissionToUser(this Stream securable, StreamAssignedPermissionsToUser permission, DatabaseUser principal, List<PermissionView> view)
        {
            if (securable == null) throw new ArgumentException("Database is null");
            if (permission == null) throw new ArgumentException("Permission are null");
            if (principal == null) throw new ArgumentException("Principal are null");
            if (view == null) throw new ArgumentException("Permission view is null");

            securable.StreamAssignedPermissionsToUsers.Add(permission);

            view.Add(new PermissionView()
            {
                ServerIdOfPrincipal = principal.ServerId,
                DatabaseIdOfPrincipal = principal.DatabaseId,
                PrincipalId = principal.DbUsrId,

                ServerIdOfSecurable = securable.ServerId,
                DatabaseIdOfSecurable = securable.DatabaseId,
                SchemaIdOfSecurable = securable.SchemaId,
                SecurableId = securable.StreamId,

                GranularPermissionId = permission.GranularPermissionId,
                SecurableClassId = permission.SecurableClassId,

                Denied = permission.Denied,
                Granted = permission.Granted,
                WithGrantOption = permission.WithGrantOption
            });

            return securable;
        }

        public static Stream AddStreamPermissionToDbRole(this Stream securable, StreamAssignedPermissionsToDBRole permission, DatabaseRole principal, List<PermissionView> view)
        {
            if (securable == null) throw new ArgumentException("Database is null");
            if (permission == null) throw new ArgumentException("Permission are null");
            if (principal == null) throw new ArgumentException("Principal are null");
            if (view == null) throw new ArgumentException("Permission view is null");

            securable.StreamAssignedPermissionsToDBRoles.Add(permission);

            view.Add(new PermissionView()
            {
                ServerIdOfPrincipal = principal.ServerId,
                DatabaseIdOfPrincipal = principal.DatabaseId,
                PrincipalId = principal.DbRoleId,

                ServerIdOfSecurable = securable.ServerId,
                DatabaseIdOfSecurable = securable.DatabaseId,
                SchemaIdOfSecurable = securable.SchemaId,
                SecurableId = securable.StreamId,

                GranularPermissionId = permission.GranularPermissionId,
                SecurableClassId = permission.SecurableClassId,

                Denied = permission.Denied,
                Granted = permission.Granted,
                WithGrantOption = permission.WithGrantOption
            });

            return securable;
        }
    }
}
