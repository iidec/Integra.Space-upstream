using EntityFramework.MoqHelper;
using Integra.Space.Database;
using Moq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integra.Space.UnitTests
{
    public class PersistentMockEnvironment
    {
        private Server CreateServer(string name)
        {
            return new Server
            {
                ServerId = Guid.NewGuid(),
                ServerName = name,
                ServerRoles = new List<ServerRole>(),
                Databases = new List<Database.Database>(),
                Logins = new List<Login>(),
                Endpoints = new List<Endpoint>(),
                ServersAssignedPermissionsToLogins = new List<ServerAssignedPermissionsToLogin>(),
                ServersAssignedPermissionsToServerRoles = new List<ServerAssignedPermissionsToServerRole>()
            };
        }

        public ServerRole CreateServerRole(string name)
        {
            return new ServerRole
            {
                ServerRoleId = Guid.NewGuid(),
                ServerRoleName = name,
                Logins = new List<Login>(),
                ServersAssignedPermissionsToServeRoles = new List<ServerAssignedPermissionsToServerRole>(),
                LoginsAssignedPermissionsToServerRoles = new List<LoginAssignedPermissionsToServerRole>(),
                EndpointsAssignedPermissionsToServerRoles = new List<EndpointAssignedPermissionsToServerRole>()
            };
        }

        public Login CreateLogin(string name, string password)
        {
            return new Login
            {
                LoginId = Guid.NewGuid(),
                LoginName = name,
                LoginPassword = password,
                IsActive = true,
                Databases = new List<Database.Database>(),
                DatabaseUsers = new List<DatabaseUser>(),
                Endpoints = new List<Endpoint>(),
                ServerRoles = new List<ServerRole>(),
                LoginsAssignedPermissionsToServerRoles = new List<LoginAssignedPermissionsToServerRole>(),
                EndpointsAssignedPermissionsToLogins = new List<EndpointAssignedPermissionsToLogin>(),
                LoginsAssignedPermissionsToLogins = new List<LoginAssignedPermissionsToLogin>(),
                ServersAssignedPermissionsToLogins = new List<ServerAssignedPermissionsToLogin>(),
                LoginsAssignedPermissionsToLogins1 = new List<LoginAssignedPermissionsToLogin>()
            };
        }

        public DatabaseUser CreateUser(string name)
        {
            return new DatabaseUser
            {
                DbUsrId = Guid.NewGuid(),
                DbUsrName = name,
                IsActive = true,
                DatabaseRoles = new List<DatabaseRole>(),
                DatabaseRoles1 = new List<DatabaseRole>(),
                DatabaseAssignedPermissionsToUsers = new List<DatabaseAssignedPermissionsToUser>(),
                DBRolesAssignedPermissionsToUsers = new List<DBRoleAssignedPermissionsToUser>(),
                SchemaAssignedPermissionsToUsers = new List<SchemaAssignedPermissionsToUser>(),
                SourceAssignedPermissionsToUsers = new List<SourceAssignedPermissionsToUser>(),
                StreamAssignedPermissionsToUsers = new List<StreamAssignedPermissionsToUser>(),
                UserAssignedPermissionsToDBRoles = new List<UserAssignedPermissionsToDBRole>(),
                UserAssignedPermissionsToUsers = new List<UserAssignedPermissionsToUsers>(),
                UserAssignedPermissionsToUsers1 = new List<UserAssignedPermissionsToUsers>(),
            };
        }

        private DatabaseUser CreateDboUser()
        {
            return this.CreateUser(PersistentMockConstants.DBO_USER_NAME);
        }

        public Database.Database CreateDatabase(string name)
        {
            return new Database.Database
            {
                DatabaseId = Guid.NewGuid(),
                DatabaseName = name,
                IsActive = true,
                DatabaseUsers = new List<DatabaseUser>(),
                Schemas = new List<Schema>(),
                DatabaseRoles = new List<DatabaseRole>(),
                Logins = new List<Login>(),
                DatabaseAssignedPermissionsToDBRoles = new List<DatabaseAssignedPermissionsToDBRole>(),
                DatabaseAssignedPermissionsToUsers = new List<DatabaseAssignedPermissionsToUser>()
            };
        }

        public DatabaseRole CreateDbRole(string name)
        {
            return new DatabaseRole
            {
                DbRoleName = name,
                DbRoleId = Guid.NewGuid(),
                IsActive = true
            };
        }

        public List<StreamColumn> CreateStreamColumn(string name, Type type)
        {
            return new List<StreamColumn>
            {
                new StreamColumn
                {
                    ColumnName = name,
                    ColumnType = type.AssemblyQualifiedName,
                    ColumnId = Guid.NewGuid()
                }
            };
        }

        public SourceColumn CreateSourceColumn(string name, Type type, int? length = null)
        {
            return new SourceColumn
            {
                ColumnId = Guid.NewGuid(),
                ColumnName = name,
                ColumnType = type.AssemblyQualifiedName,
                ColumnIndex = 0,
                ColumnLength = length
            };
        }

        public Schema CreateSchema(string name)
        {
            return new Schema
            {
                SchemaId = Guid.NewGuid(),
                SchemaName = name,
                Sources = new List<Source>(),
                Streams = new List<Stream>(),
                DatabaseUsers = new List<DatabaseUser>(),
                SchemaAssignedPermissionsToDBRoles = new List<SchemaAssignedPermissionsToDBRole>(),
                SchemaAssignedPermissionsToUsers = new List<SchemaAssignedPermissionsToUser>()
            };
        }

        public Stream CreateStream(string name, string query, byte[] assembly)
        {
            return new Stream
            {
                StreamId = Guid.NewGuid(),
                StreamName = name,
                Assembly = assembly,
                Query = query,
                IsActive = true,
                ProjectionColumns = new List<StreamColumn>(),
                Sources = new List<SourceByStream>(),
                StreamAssignedPermissionsToDBRoles = new List<StreamAssignedPermissionsToDBRole>(),
                StreamAssignedPermissionsToUsers = new List<StreamAssignedPermissionsToUser>()
            };
        }

        public Source CreateSource(string name)
        {
            return new Source
            {
                SourceId = Guid.NewGuid(),
                SourceName = name,
                IsActive = true,
                CacheDurability = 60,
                CacheSize = 1000,
                Persistent = true,
                Columns = new List<SourceColumn>(),
                Streams = new List<SourceByStream>(),
                SourceAssignedPermissionsToDBRoles = new List<SourceAssignedPermissionsToDBRole>(),
                SourceAssignedPermissionsToUsers = new List<SourceAssignedPermissionsToUser>()
            };
        }

        private Tuple<Server, List<PermissionView>> CreateInitialState()
        {
            Server server = this.CreateServer(PersistentMockConstants.TEST_SERVER_NAME);
            ServerRole sysAdmin = this.CreateSysAdminServerRole();
            ServerRole sysReader = this.CreateSysReaderServerRole();
            Login sa = this.CreateSaLogin();
            Database.Database master = this.CreateMasterDatabase();
            Schema dboSchema = this.CreateDboSchema();
            DatabaseUser dboUser = this.CreateDboUser();
            Source inputSource = this.CreateDefaultInputSource();
            Source outputSource = this.CreateDefaultOutputSource();
            Stream stream = this.CreateStream(PersistentMockConstants.TEST_STREAM_NAME, PersistentMockConstants.QUERY_TEST_STREAM, PersistentMockConstants.ASSEMBLY_TEST_STREAM);

            List<PermissionView> allPermissions = new List<PermissionView>();

            server
                .AddServerRole(sysAdmin)
                .AddServerRole(sysReader)
                .AddDatabase(master, sa)
                .AddLogin(sa, master, sysAdmin);

            master
                .AddSchema(dboSchema, dboUser)
                .AddUser(dboUser, sa, dboSchema);

            dboSchema
                .AddSource(inputSource, dboUser)
                .AddSource(outputSource, dboUser)
                .AddStream(stream, dboUser, inputSource, outputSource);

            inputSource
                .AddSourceColumn(this.CreateInputSourceColumns().ToArray());

            outputSource
                .AddSourceColumn(this.CreateOutputSourceColumns().ToArray());

            stream
                .AddStreamColumn(this.CreateStreamColumn(PersistentMockConstants.TEST_STREAM_COLUMN_NAME, PersistentMockConstants.TEST_STREAM_COLUMN_TYPE).ToArray());

            return Tuple.Create(server, allPermissions);
        }

        private Tuple<Server, List<PermissionView>> CreateExtendedState()
        {
            Server server = this.CreateServer(PersistentMockConstants.TEST_SERVER_NAME);
            ServerRole sysAdmin = this.CreateSysAdminServerRole();
            ServerRole sysReader = this.CreateSysReaderServerRole();

            Login sa = this.CreateSaLogin();
            Login adminLogin1 = this.CreateLogin(PersistentMockConstants.ADMIN_LOGIN_1_NAME, PersistentMockConstants.ADMIN_LOGIN_1_PASSWORD);
            Login adminLogin2 = this.CreateLogin(PersistentMockConstants.ADMIN_LOGIN_2_NAME, PersistentMockConstants.ADMIN_LOGIN_2_PASSWORD);
            Login normalLogin1 = this.CreateLogin(PersistentMockConstants.NORMAL_LOGIN_1_NAME, PersistentMockConstants.NORMAL_LOGIN_1_PASSWORD);
            Login normalLogin2 = this.CreateLogin(PersistentMockConstants.NORMAL_LOGIN_2_NAME, PersistentMockConstants.NORMAL_LOGIN_2_PASSWORD);
            Login normalLogin3 = this.CreateLogin(PersistentMockConstants.NORMAL_LOGIN_3_NAME, PersistentMockConstants.NORMAL_LOGIN_3_PASSWORD);

            Database.Database master = this.CreateMasterDatabase();
            Database.Database tests = this.CreateDatabase(PersistentMockConstants.TEST_DATABASE_NAME);

            DatabaseUser dboUser = this.CreateDboUser();
            DatabaseUser adminUser1 = this.CreateUser(PersistentMockConstants.ADMIN_USER1_NAME);
            DatabaseUser adminUser2 = this.CreateUser(PersistentMockConstants.ADMIN_USER2_NAME);
            DatabaseUser normalUser1 = this.CreateUser(PersistentMockConstants.NORMAL_USER_1_NAME);
            DatabaseUser normalUser2 = this.CreateUser(PersistentMockConstants.NORMAL_USER_2_NAME);
            DatabaseUser normalUser3 = this.CreateUser(PersistentMockConstants.NORMAL_USER_3_NAME);

            DatabaseRole role1 = this.CreateDbRole(PersistentMockConstants.ROLE_1_NAME);
            DatabaseRole role2 = this.CreateDbRole(PersistentMockConstants.ROLE_2_NAME);

            Schema dboSchema = this.CreateDboSchema();
            Schema testSchema = this.CreateSchema(PersistentMockConstants.TEST_SCHEMA_NAME);

            Source inputSource = this.CreateDefaultInputSource();
            Source outputSource = this.CreateDefaultOutputSource();
            Stream stream = this.CreateStream(PersistentMockConstants.TEST_STREAM_NAME, PersistentMockConstants.QUERY_TEST_STREAM, PersistentMockConstants.ASSEMBLY_TEST_STREAM);

            List<PermissionView> allPermissions = new List<PermissionView>();

            server
                .AddServerRole(sysAdmin)
                .AddServerRole(sysReader)
                .AddDatabase(master, sa)
                .AddDatabase(tests, sa)
                .AddLogin(sa, master, sysAdmin)
                .AddLogin(adminLogin1, master, sysAdmin)
                .AddLogin(adminLogin2, master, sysAdmin)
                .AddLogin(normalLogin1, master, null)
                .AddLogin(normalLogin2, master, null)
                .AddLogin(normalLogin3, master, null)
                ;

            master
                .AddSchema(dboSchema, dboUser)
                .AddSchema(testSchema, dboUser)
                .AddUser(dboUser, sa, dboSchema)
                .AddUser(adminUser1, adminLogin1, dboSchema)
                .AddUser(adminUser2, adminLogin2, testSchema)
                .AddUser(normalUser1, normalLogin1, dboSchema)
                .AddUser(normalUser2, normalLogin2, dboSchema)
                .AddUser(normalUser3, normalLogin3, testSchema)
                .AddRole(role1, dboUser)
                .AddRole(role2, dboUser)
                ;

            dboSchema
                .AddSource(inputSource, dboUser)
                .AddSource(outputSource, dboUser)
                .AddStream(stream, dboUser, inputSource, outputSource);

            inputSource
                .AddSourceColumn(this.CreateInputSourceColumns().ToArray());

            outputSource
                .AddSourceColumn(this.CreateOutputSourceColumns().ToArray());

            stream
                .AddStreamColumn(this.CreateStreamColumn(PersistentMockConstants.TEST_STREAM_COLUMN_NAME, PersistentMockConstants.TEST_STREAM_COLUMN_TYPE).ToArray());

            return Tuple.Create(server, allPermissions);
        }

        public Mock<SpaceDbContext> BuildMinimunEnvironment()
        {
            return this.BuildEnvironment(extended: false);
        }
        public Mock<SpaceDbContext> BuildExtendedEnvironment()
        {
            return this.BuildEnvironment(extended: true);
        }

        private Mock<SpaceDbContext> BuildEnvironment(bool extended)
        {
            Server server = null;
            List<PermissionView> allPermissions = null;

            if (extended)
            {
                Tuple<Server, List<PermissionView>> systemObjects = this.CreateExtendedState();
                server = systemObjects.Item1;
                allPermissions = systemObjects.Item2;
            }
            else
            {
                Tuple<Server, List<PermissionView>> systemObjects = this.CreateInitialState();
                server = systemObjects.Item1;
                allPermissions = systemObjects.Item2;
            }

            List<SecurableClass> securableClasses = this.CreateSecurableClasses();
            List<GranularPermission> granularPermissions = this.CreateGranularPermissions();
            List<PermissionBySecurable> permissions = this.CreatePermissions(securableClasses, granularPermissions);

            #region creación de mock dbsets

            var mockServerSet = this.CreateMockForDbSet<Server>(new List<Server> { server }).WithFind(new List<Server> { server }, "ServerId");
            var mockServerRoleSet = this.CreateMockForDbSet<ServerRole>(server.ServerRoles.ToList());
            var mockDatabaseSet = this.CreateMockForDbSet<Database.Database>(server.Databases.ToList());
            var mockLoginSet = this.CreateMockForDbSet<Login>(server.Logins.ToList());
            var mockUsersSet = this.CreateMockForDbSet<DatabaseUser>(server.Databases.SelectMany(x => x.DatabaseUsers).ToList());
            var mockDatabaseRoleSet = this.CreateMockForDbSet<DatabaseRole>(server.Databases.SelectMany(x => x.DatabaseRoles).ToList());
            var mockSchemaSet = this.CreateMockForDbSet<Schema>(server.Databases.SelectMany(x => x.Schemas).ToList());
            var mockSourceSet = this.CreateMockForDbSet<Source>(server.Databases.SelectMany(x => x.Schemas).SelectMany(x => x.Sources).ToList());
            var mockSourceColumnSet = this.CreateMockForDbSet<SourceColumn>(server.Databases.SelectMany(x => x.Schemas).SelectMany(x => x.Sources).SelectMany(x => x.Columns).ToList());
            var mockStreamSet = this.CreateMockForDbSet<Stream>(server.Databases.SelectMany(x => x.Schemas).SelectMany(x => x.Streams).ToList());
            var mockStreamColumnsSet = this.CreateMockForDbSet<StreamColumn>(server.Databases.SelectMany(x => x.Schemas).SelectMany(x => x.Streams).SelectMany(x => x.ProjectionColumns).ToList());

            var mockSecurableClassSet = this.CreateMockForDbSet<SecurableClass>(securableClasses);
            var mockGranularPermissionSet = this.CreateMockForDbSet<GranularPermission>(granularPermissions);
            var mockPermissionBySecurableSet = this.CreateMockForDbSet<PermissionBySecurable>(permissions);
            var mockHierarchyPermissionsSet = this.CreateMockForDbSet<HierarchyPermissions>(this.CreatePermissionHierarchy(permissions));

            #region permissions over objects

            var mockDatabaseAssignedPermissionsToDBRoleSet = this.CreateMockForDbSet<DatabaseAssignedPermissionsToDBRole>(server.Databases.SelectMany(x => x.DatabaseAssignedPermissionsToDBRoles).ToList());
            var mockDatabaseAssignedPermissionsToUserSet = this.CreateMockForDbSet<DatabaseAssignedPermissionsToUser>(server.Databases.SelectMany(x => x.DatabaseAssignedPermissionsToUsers).ToList());

            var mockDbRolesAssignedPermissionsToDBRoleSet = this.CreateMockForDbSet<DBRoleAssignedPermissionsToDBRole>(server.Databases.SelectMany(x => x.DatabaseRoles).SelectMany(x => x.DBRolesAssignedPermissionsToDBRoles).ToList());
            var mockDbRolesAssignedPermissionsToUserSet = this.CreateMockForDbSet<DBRoleAssignedPermissionsToUser>(server.Databases.SelectMany(x => x.DatabaseRoles).SelectMany(x => x.DBRolesAssignedPermissionsToUsers).ToList());

            var mockLoginAssignedPermissionsToLoginSet = this.CreateMockForDbSet<LoginAssignedPermissionsToLogin>(server.Logins.SelectMany(x => x.LoginsAssignedPermissionsToLogins).ToList());
            var mockLoginAssignedPermissionsToServerRoleSet = this.CreateMockForDbSet<LoginAssignedPermissionsToServerRole>(server.Logins.SelectMany(x => x.LoginsAssignedPermissionsToServerRoles).ToList());

            var mockSchemaAssignedPermissionsToDBRoleSet = this.CreateMockForDbSet<SchemaAssignedPermissionsToDBRole>(server.Databases.SelectMany(x => x.Schemas).SelectMany(x => x.SchemaAssignedPermissionsToDBRoles).ToList());
            var mockSchemaAssignedPermissionsToUserSet = this.CreateMockForDbSet<SchemaAssignedPermissionsToUser>(server.Databases.SelectMany(x => x.Schemas).SelectMany(x => x.SchemaAssignedPermissionsToUsers).ToList());

            var mockServerAssignedPermissionsToLoginSet = this.CreateMockForDbSet<ServerAssignedPermissionsToLogin>(server.ServersAssignedPermissionsToLogins.ToList());
            var mockServerAssignedPermissionsToServerRoleSet = this.CreateMockForDbSet<ServerAssignedPermissionsToServerRole>(server.ServersAssignedPermissionsToServerRoles.ToList());

            var mockSourceAssignedPermissionsToDBRoleSet = this.CreateMockForDbSet<SourceAssignedPermissionsToDBRole>(server.Databases.SelectMany(x => x.Schemas).SelectMany(x => x.Sources).SelectMany(x => x.SourceAssignedPermissionsToDBRoles).ToList());
            var mockSourceAssignedPermissionsToUserSet = this.CreateMockForDbSet<SourceAssignedPermissionsToUser>(server.Databases.SelectMany(x => x.Schemas).SelectMany(x => x.Sources).SelectMany(x => x.SourceAssignedPermissionsToUsers).ToList());

            var mockStreamAssignedPermissionsToDBRoleSet = this.CreateMockForDbSet<StreamAssignedPermissionsToDBRole>(server.Databases.SelectMany(x => x.Schemas).SelectMany(x => x.Streams).SelectMany(x => x.StreamAssignedPermissionsToDBRoles).ToList());
            var mockStreamAssignedPermissionsToUserSet = this.CreateMockForDbSet<StreamAssignedPermissionsToUser>(server.Databases.SelectMany(x => x.Schemas).SelectMany(x => x.Streams).SelectMany(x => x.StreamAssignedPermissionsToUsers).ToList());

            var mockUserAssignedPermissionsToDBRoleSet = this.CreateMockForDbSet<UserAssignedPermissionsToDBRole>(server.Databases.SelectMany(x => x.DatabaseUsers).SelectMany(x => x.UserAssignedPermissionsToDBRoles).ToList());
            var mockUserAssignedPermissionsToUserSet = this.CreateMockForDbSet<UserAssignedPermissionsToUsers>(server.Databases.SelectMany(x => x.DatabaseUsers).SelectMany(x => x.UserAssignedPermissionsToUsers).ToList());


            var mockPermissionViewSet = this.CreateMockForDbSet<PermissionView>(allPermissions);

            #endregion permissions over objects

            #endregion creación de mock dbsets

            var mockDbContext = EntityFrameworkMoqHelper.CreateMockForDbContext<SpaceDbContext>();
            mockDbContext.Setup(x => x.Servers).Returns(mockServerSet.Object);
            mockDbContext.Setup(x => x.Databases).Returns(mockDatabaseSet.Object);
            mockDbContext.Setup(x => x.Logins).Returns(mockLoginSet.Object);
            mockDbContext.Setup(x => x.ServerRoles).Returns(mockServerRoleSet.Object);
            mockDbContext.Setup(x => x.DatabaseUsers).Returns(mockUsersSet.Object);
            mockDbContext.Setup(x => x.DatabaseRoles).Returns(mockDatabaseRoleSet.Object);
            mockDbContext.Setup(x => x.Schemas).Returns(mockSchemaSet.Object);
            mockDbContext.Setup(x => x.Sources).Returns(mockSourceSet.Object);
            mockDbContext.Setup(x => x.SourceColumns).Returns(mockSourceColumnSet.Object);
            mockDbContext.Setup(x => x.Streams).Returns(mockStreamSet.Object);
            mockDbContext.Setup(x => x.StreamColumns).Returns(mockStreamColumnsSet.Object);
            mockDbContext.Setup(x => x.SecurableClasses).Returns(mockSecurableClassSet.Object);
            mockDbContext.Setup(x => x.GranularPermissions).Returns(mockGranularPermissionSet.Object);
            mockDbContext.Setup(x => x.PermissionsBySecurables).Returns(mockPermissionBySecurableSet.Object);
            mockDbContext.Setup(x => x.HierarchyPermissions).Returns(mockHierarchyPermissionsSet.Object);

            mockDbContext.Setup(x => x.ServersAssignedPermissionsToLogins).Returns(mockServerAssignedPermissionsToLoginSet.Object);
            mockDbContext.Setup(x => x.ServersAssignedPermissionsToServerRoles).Returns(mockServerAssignedPermissionsToServerRoleSet.Object);
            mockDbContext.Setup(x => x.LoginsAssignedPermissionsToLogins).Returns(mockLoginAssignedPermissionsToLoginSet.Object);
            mockDbContext.Setup(x => x.LoginsAssignedPermissionsToServerRoles).Returns(mockLoginAssignedPermissionsToServerRoleSet.Object);
            mockDbContext.Setup(x => x.DatabaseAssignedPermissionsToDBRoles).Returns(mockDatabaseAssignedPermissionsToDBRoleSet.Object);
            mockDbContext.Setup(x => x.DatabaseAssignedPermissionsToUsers).Returns(mockDatabaseAssignedPermissionsToUserSet.Object);
            mockDbContext.Setup(x => x.UserAssignedPermissionsToDBRoles).Returns(mockUserAssignedPermissionsToDBRoleSet.Object);
            mockDbContext.Setup(x => x.UserAssignedPermissionsToUsers).Returns(mockUserAssignedPermissionsToUserSet.Object);
            mockDbContext.Setup(x => x.SchemasAssignedPermissionsToDbRoles).Returns(mockSchemaAssignedPermissionsToDBRoleSet.Object);
            mockDbContext.Setup(x => x.SchemaAssignedPermissionsToUsers).Returns(mockSchemaAssignedPermissionsToUserSet.Object);
            mockDbContext.Setup(x => x.DBRolesAssignedPermissionsToDBRoles).Returns(mockDbRolesAssignedPermissionsToDBRoleSet.Object);
            mockDbContext.Setup(x => x.DBRolesAssignedPermissionsToUsers).Returns(mockDbRolesAssignedPermissionsToUserSet.Object);
            mockDbContext.Setup(x => x.SourceAssignedPermissionsToDBRoles).Returns(mockSourceAssignedPermissionsToDBRoleSet.Object);
            mockDbContext.Setup(x => x.SourceAssignedPermissionsToUsers).Returns(mockSourceAssignedPermissionsToUserSet.Object);
            mockDbContext.Setup(x => x.StreamAssignedPermissionsToDBRoles).Returns(mockStreamAssignedPermissionsToDBRoleSet.Object);
            mockDbContext.Setup(x => x.StreamAssignedPermissionsToUsers).Returns(mockStreamAssignedPermissionsToUserSet.Object);
            mockDbContext.Setup(x => x.VWPermissions).Returns(mockPermissionViewSet.Object);

            return mockDbContext;
        }

        private Moq.Mock<DbSet<TSystemObject>> CreateMockForDbSet<TSystemObject>(List<TSystemObject> collection) where TSystemObject : class
        {
            return EntityFrameworkMoqHelper.CreateMockForDbSet<TSystemObject>()
                                                            .SetupForQueryOn(collection)
                                                            .WithAdd(collection)                                                            
                                                            .WithRemove(collection);
        }

        private List<HierarchyPermissions> CreatePermissionHierarchy(List<PermissionBySecurable> permissions)
        {
            HierarchyPermissions hp1 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("ED9F18F4-BFDD-4DF6-BBD1-3CF33E294769") && x.GranularPermissionId == Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659")), SecurableClassId = Guid.Parse("ED9F18F4-BFDD-4DF6-BBD1-3CF33E294769"), GranularPermissionId = Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("ED9F18F4-BFDD-4DF6-BBD1-3CF33E294769") && x.GranularPermissionId == Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88")), ParentSecurableClassId = Guid.Parse("ED9F18F4-BFDD-4DF6-BBD1-3CF33E294769"), ParentGranularPermissionId = Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88"), Id = Guid.Parse("2106A420-9FF4-42BC-B7FF-14CB04A6D665") };
            HierarchyPermissions hp2 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("ED9F18F4-BFDD-4DF6-BBD1-3CF33E294769") && x.GranularPermissionId == Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659")), SecurableClassId = Guid.Parse("ED9F18F4-BFDD-4DF6-BBD1-3CF33E294769"), GranularPermissionId = Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD") && x.GranularPermissionId == Guid.Parse("159F7116-A56C-4507-9FD2-27FA2DC26E87")), ParentSecurableClassId = Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD"), ParentGranularPermissionId = Guid.Parse("159F7116-A56C-4507-9FD2-27FA2DC26E87"), Id = Guid.Parse("CCE751AC-F7A9-4B6A-9FC8-59466A95DB8B") };
            HierarchyPermissions hp3 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("ED9F18F4-BFDD-4DF6-BBD1-3CF33E294769") && x.GranularPermissionId == Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC")), SecurableClassId = Guid.Parse("ED9F18F4-BFDD-4DF6-BBD1-3CF33E294769"), GranularPermissionId = Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("ED9F18F4-BFDD-4DF6-BBD1-3CF33E294769") && x.GranularPermissionId == Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88")), ParentSecurableClassId = Guid.Parse("ED9F18F4-BFDD-4DF6-BBD1-3CF33E294769"), ParentGranularPermissionId = Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88"), Id = Guid.Parse("4ACEDBC0-DD0A-4D85-9808-CBFCCFDEEB63") };
            HierarchyPermissions hp4 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("ED9F18F4-BFDD-4DF6-BBD1-3CF33E294769") && x.GranularPermissionId == Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88")), SecurableClassId = Guid.Parse("ED9F18F4-BFDD-4DF6-BBD1-3CF33E294769"), GranularPermissionId = Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD") && x.GranularPermissionId == Guid.Parse("CB6D08BD-B510-4405-A193-8D3D85575618")), ParentSecurableClassId = Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD"), ParentGranularPermissionId = Guid.Parse("CB6D08BD-B510-4405-A193-8D3D85575618"), Id = Guid.Parse("4AF5E727-D36E-463F-BFE6-5985B3DB015C") };
            HierarchyPermissions hp5 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("ED9F18F4-BFDD-4DF6-BBD1-3CF33E294769") && x.GranularPermissionId == Guid.Parse("E50A90EA-E6AF-454E-9E7E-B6427B61F950")), SecurableClassId = Guid.Parse("ED9F18F4-BFDD-4DF6-BBD1-3CF33E294769"), GranularPermissionId = Guid.Parse("E50A90EA-E6AF-454E-9E7E-B6427B61F950"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("ED9F18F4-BFDD-4DF6-BBD1-3CF33E294769") && x.GranularPermissionId == Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88")), ParentSecurableClassId = Guid.Parse("ED9F18F4-BFDD-4DF6-BBD1-3CF33E294769"), ParentGranularPermissionId = Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88"), Id = Guid.Parse("3CE0D868-B5D9-4A06-9C63-C8F6B5C1224C") };
            HierarchyPermissions hp6 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("ED9F18F4-BFDD-4DF6-BBD1-3CF33E294769") && x.GranularPermissionId == Guid.Parse("96FFAEDA-F2E9-4056-B394-C63D56DF38B5")), SecurableClassId = Guid.Parse("ED9F18F4-BFDD-4DF6-BBD1-3CF33E294769"), GranularPermissionId = Guid.Parse("96FFAEDA-F2E9-4056-B394-C63D56DF38B5"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("ED9F18F4-BFDD-4DF6-BBD1-3CF33E294769") && x.GranularPermissionId == Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88")), ParentSecurableClassId = Guid.Parse("ED9F18F4-BFDD-4DF6-BBD1-3CF33E294769"), ParentGranularPermissionId = Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88"), Id = Guid.Parse("646CC129-80B1-41B4-BC62-2FE3DB86B451") };
            HierarchyPermissions hp7 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("236A8A3F-C8AC-4047-B79E-4FECF5E691D4") && x.GranularPermissionId == Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659")), SecurableClassId = Guid.Parse("236A8A3F-C8AC-4047-B79E-4FECF5E691D4"), GranularPermissionId = Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("236A8A3F-C8AC-4047-B79E-4FECF5E691D4") && x.GranularPermissionId == Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88")), ParentSecurableClassId = Guid.Parse("236A8A3F-C8AC-4047-B79E-4FECF5E691D4"), ParentGranularPermissionId = Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88"), Id = Guid.Parse("F83D3E23-7912-4C82-9792-CAB924C568AF") };
            HierarchyPermissions hp8 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("236A8A3F-C8AC-4047-B79E-4FECF5E691D4") && x.GranularPermissionId == Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659")), SecurableClassId = Guid.Parse("236A8A3F-C8AC-4047-B79E-4FECF5E691D4"), GranularPermissionId = Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3") && x.GranularPermissionId == Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659")), ParentSecurableClassId = Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3"), ParentGranularPermissionId = Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659"), Id = Guid.Parse("E9EDD742-0007-4731-BF59-BE2EFBA875B2") };
            HierarchyPermissions hp9 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("236A8A3F-C8AC-4047-B79E-4FECF5E691D4") && x.GranularPermissionId == Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC")), SecurableClassId = Guid.Parse("236A8A3F-C8AC-4047-B79E-4FECF5E691D4"), GranularPermissionId = Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("236A8A3F-C8AC-4047-B79E-4FECF5E691D4") && x.GranularPermissionId == Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88")), ParentSecurableClassId = Guid.Parse("236A8A3F-C8AC-4047-B79E-4FECF5E691D4"), ParentGranularPermissionId = Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88"), Id = Guid.Parse("D3378642-79DE-4A3B-9652-3ACB53BDC106") };
            HierarchyPermissions hp10 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("236A8A3F-C8AC-4047-B79E-4FECF5E691D4") && x.GranularPermissionId == Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC")), SecurableClassId = Guid.Parse("236A8A3F-C8AC-4047-B79E-4FECF5E691D4"), GranularPermissionId = Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3") && x.GranularPermissionId == Guid.Parse("B8092C8F-3110-4A07-B2C4-100FBAA23FD0")), ParentSecurableClassId = Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3"), ParentGranularPermissionId = Guid.Parse("B8092C8F-3110-4A07-B2C4-100FBAA23FD0"), Id = Guid.Parse("186BD8F7-C437-49E6-97CC-2BA3C50F61AE") };
            HierarchyPermissions hp11 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("236A8A3F-C8AC-4047-B79E-4FECF5E691D4") && x.GranularPermissionId == Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88")), SecurableClassId = Guid.Parse("236A8A3F-C8AC-4047-B79E-4FECF5E691D4"), GranularPermissionId = Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3") && x.GranularPermissionId == Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88")), ParentSecurableClassId = Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3"), ParentGranularPermissionId = Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88"), Id = Guid.Parse("C4615DD1-23D5-4866-8995-0ABC59974628") };
            HierarchyPermissions hp12 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("6FB2683D-544A-4DD7-9077-5A6E45FB9AE5") && x.GranularPermissionId == Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659")), SecurableClassId = Guid.Parse("6FB2683D-544A-4DD7-9077-5A6E45FB9AE5"), GranularPermissionId = Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("6FB2683D-544A-4DD7-9077-5A6E45FB9AE5") && x.GranularPermissionId == Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88")), ParentSecurableClassId = Guid.Parse("6FB2683D-544A-4DD7-9077-5A6E45FB9AE5"), ParentGranularPermissionId = Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88"), Id = Guid.Parse("B90E71B4-67EE-448C-9FDE-F85EDCC77A2B") };
            HierarchyPermissions hp13 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("6FB2683D-544A-4DD7-9077-5A6E45FB9AE5") && x.GranularPermissionId == Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659")), SecurableClassId = Guid.Parse("6FB2683D-544A-4DD7-9077-5A6E45FB9AE5"), GranularPermissionId = Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("C6760E1C-03E3-4FCE-ADD2-9C112D8F222A") && x.GranularPermissionId == Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659")), ParentSecurableClassId = Guid.Parse("C6760E1C-03E3-4FCE-ADD2-9C112D8F222A"), ParentGranularPermissionId = Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659"), Id = Guid.Parse("54C35913-BF9C-4D13-A905-C3B25247D7AB") };
            HierarchyPermissions hp14 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("6FB2683D-544A-4DD7-9077-5A6E45FB9AE5") && x.GranularPermissionId == Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC")), SecurableClassId = Guid.Parse("6FB2683D-544A-4DD7-9077-5A6E45FB9AE5"), GranularPermissionId = Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("6FB2683D-544A-4DD7-9077-5A6E45FB9AE5") && x.GranularPermissionId == Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88")), ParentSecurableClassId = Guid.Parse("6FB2683D-544A-4DD7-9077-5A6E45FB9AE5"), ParentGranularPermissionId = Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88"), Id = Guid.Parse("0C9F5DEC-7F12-4741-9A55-E81444DFE096") };
            HierarchyPermissions hp15 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("6FB2683D-544A-4DD7-9077-5A6E45FB9AE5") && x.GranularPermissionId == Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC")), SecurableClassId = Guid.Parse("6FB2683D-544A-4DD7-9077-5A6E45FB9AE5"), GranularPermissionId = Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("C6760E1C-03E3-4FCE-ADD2-9C112D8F222A") && x.GranularPermissionId == Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC")), ParentSecurableClassId = Guid.Parse("C6760E1C-03E3-4FCE-ADD2-9C112D8F222A"), ParentGranularPermissionId = Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC"), Id = Guid.Parse("E19F6DE4-1A4B-4447-B8F6-7EE6FE3668A5") };
            HierarchyPermissions hp16 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("6FB2683D-544A-4DD7-9077-5A6E45FB9AE5") && x.GranularPermissionId == Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88")), SecurableClassId = Guid.Parse("6FB2683D-544A-4DD7-9077-5A6E45FB9AE5"), GranularPermissionId = Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("C6760E1C-03E3-4FCE-ADD2-9C112D8F222A") && x.GranularPermissionId == Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88")), ParentSecurableClassId = Guid.Parse("C6760E1C-03E3-4FCE-ADD2-9C112D8F222A"), ParentGranularPermissionId = Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88"), Id = Guid.Parse("38CEB389-515F-45E0-B4D3-047D439FAC90") };
            HierarchyPermissions hp17 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("6FB2683D-544A-4DD7-9077-5A6E45FB9AE5") && x.GranularPermissionId == Guid.Parse("0E46C67A-8145-40D9-A6CF-7C0EED7A1442")), SecurableClassId = Guid.Parse("6FB2683D-544A-4DD7-9077-5A6E45FB9AE5"), GranularPermissionId = Guid.Parse("0E46C67A-8145-40D9-A6CF-7C0EED7A1442"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("6FB2683D-544A-4DD7-9077-5A6E45FB9AE5") && x.GranularPermissionId == Guid.Parse("C41262CD-C9C2-492C-8FC0-8F2E031CC99F")), ParentSecurableClassId = Guid.Parse("6FB2683D-544A-4DD7-9077-5A6E45FB9AE5"), ParentGranularPermissionId = Guid.Parse("C41262CD-C9C2-492C-8FC0-8F2E031CC99F"), Id = Guid.Parse("22E45321-23B9-4021-9C7A-6771A49CDCB7") };
            HierarchyPermissions hp18 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("6FB2683D-544A-4DD7-9077-5A6E45FB9AE5") && x.GranularPermissionId == Guid.Parse("C41262CD-C9C2-492C-8FC0-8F2E031CC99F")), SecurableClassId = Guid.Parse("6FB2683D-544A-4DD7-9077-5A6E45FB9AE5"), GranularPermissionId = Guid.Parse("C41262CD-C9C2-492C-8FC0-8F2E031CC99F"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("6FB2683D-544A-4DD7-9077-5A6E45FB9AE5") && x.GranularPermissionId == Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC")), ParentSecurableClassId = Guid.Parse("6FB2683D-544A-4DD7-9077-5A6E45FB9AE5"), ParentGranularPermissionId = Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC"), Id = Guid.Parse("72F11574-0359-4519-B4E4-A43F529DBBA7") };
            HierarchyPermissions hp19 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("6FB2683D-544A-4DD7-9077-5A6E45FB9AE5") && x.GranularPermissionId == Guid.Parse("96FFAEDA-F2E9-4056-B394-C63D56DF38B5")), SecurableClassId = Guid.Parse("6FB2683D-544A-4DD7-9077-5A6E45FB9AE5"), GranularPermissionId = Guid.Parse("96FFAEDA-F2E9-4056-B394-C63D56DF38B5"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("6FB2683D-544A-4DD7-9077-5A6E45FB9AE5") && x.GranularPermissionId == Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88")), ParentSecurableClassId = Guid.Parse("6FB2683D-544A-4DD7-9077-5A6E45FB9AE5"), ParentGranularPermissionId = Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88"), Id = Guid.Parse("00DFB560-3318-444D-95F9-68327FBEF7AA") };
            HierarchyPermissions hp20 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("E4F6BC86-4909-41A6-BA68-632DBEF8D6D7") && x.GranularPermissionId == Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659")), SecurableClassId = Guid.Parse("E4F6BC86-4909-41A6-BA68-632DBEF8D6D7"), GranularPermissionId = Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("E4F6BC86-4909-41A6-BA68-632DBEF8D6D7") && x.GranularPermissionId == Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88")), ParentSecurableClassId = Guid.Parse("E4F6BC86-4909-41A6-BA68-632DBEF8D6D7"), ParentGranularPermissionId = Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88"), Id = Guid.Parse("FB452337-2BEA-4514-9C8E-D81B331793B1") };
            HierarchyPermissions hp21 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("E4F6BC86-4909-41A6-BA68-632DBEF8D6D7") && x.GranularPermissionId == Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659")), SecurableClassId = Guid.Parse("E4F6BC86-4909-41A6-BA68-632DBEF8D6D7"), GranularPermissionId = Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("C6760E1C-03E3-4FCE-ADD2-9C112D8F222A") && x.GranularPermissionId == Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659")), ParentSecurableClassId = Guid.Parse("C6760E1C-03E3-4FCE-ADD2-9C112D8F222A"), ParentGranularPermissionId = Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659"), Id = Guid.Parse("BA41BF2C-D1DC-4D2A-BF33-BC5AE202750F") };
            HierarchyPermissions hp22 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("E4F6BC86-4909-41A6-BA68-632DBEF8D6D7") && x.GranularPermissionId == Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC")), SecurableClassId = Guid.Parse("E4F6BC86-4909-41A6-BA68-632DBEF8D6D7"), GranularPermissionId = Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("E4F6BC86-4909-41A6-BA68-632DBEF8D6D7") && x.GranularPermissionId == Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88")), ParentSecurableClassId = Guid.Parse("E4F6BC86-4909-41A6-BA68-632DBEF8D6D7"), ParentGranularPermissionId = Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88"), Id = Guid.Parse("874217F3-1C88-4A25-A0AB-4138A238624F") };
            HierarchyPermissions hp23 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("E4F6BC86-4909-41A6-BA68-632DBEF8D6D7") && x.GranularPermissionId == Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC")), SecurableClassId = Guid.Parse("E4F6BC86-4909-41A6-BA68-632DBEF8D6D7"), GranularPermissionId = Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("C6760E1C-03E3-4FCE-ADD2-9C112D8F222A") && x.GranularPermissionId == Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC")), ParentSecurableClassId = Guid.Parse("C6760E1C-03E3-4FCE-ADD2-9C112D8F222A"), ParentGranularPermissionId = Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC"), Id = Guid.Parse("514A1F5C-C01B-4FE2-B3E7-65C755C17F7C") };
            HierarchyPermissions hp24 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("E4F6BC86-4909-41A6-BA68-632DBEF8D6D7") && x.GranularPermissionId == Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88")), SecurableClassId = Guid.Parse("E4F6BC86-4909-41A6-BA68-632DBEF8D6D7"), GranularPermissionId = Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("C6760E1C-03E3-4FCE-ADD2-9C112D8F222A") && x.GranularPermissionId == Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88")), ParentSecurableClassId = Guid.Parse("C6760E1C-03E3-4FCE-ADD2-9C112D8F222A"), ParentGranularPermissionId = Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88"), Id = Guid.Parse("CE230FE4-951B-4908-AFEB-5055F92DA621") };
            HierarchyPermissions hp25 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3") && x.GranularPermissionId == Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659")), SecurableClassId = Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3"), GranularPermissionId = Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3") && x.GranularPermissionId == Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88")), ParentSecurableClassId = Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3"), ParentGranularPermissionId = Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88"), Id = Guid.Parse("8E599F98-700C-4730-AE28-805B7D31A030") };
            HierarchyPermissions hp26 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3") && x.GranularPermissionId == Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659")), SecurableClassId = Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3"), GranularPermissionId = Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD") && x.GranularPermissionId == Guid.Parse("7B9116C8-76F6-4002-8910-9892A4BADFB8")), ParentSecurableClassId = Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD"), ParentGranularPermissionId = Guid.Parse("7B9116C8-76F6-4002-8910-9892A4BADFB8"), Id = Guid.Parse("48E9ED7D-3960-4F99-890D-A82B7AE5C059") };
            HierarchyPermissions hp27 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3") && x.GranularPermissionId == Guid.Parse("B8092C8F-3110-4A07-B2C4-100FBAA23FD0")), SecurableClassId = Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3"), GranularPermissionId = Guid.Parse("B8092C8F-3110-4A07-B2C4-100FBAA23FD0"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3") && x.GranularPermissionId == Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC")), ParentSecurableClassId = Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3"), ParentGranularPermissionId = Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC"), Id = Guid.Parse("AB7F2F17-E154-46E8-A2F6-299F5EDADA08") };
            HierarchyPermissions hp28 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3") && x.GranularPermissionId == Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC")), SecurableClassId = Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3"), GranularPermissionId = Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3") && x.GranularPermissionId == Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88")), ParentSecurableClassId = Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3"), ParentGranularPermissionId = Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88"), Id = Guid.Parse("9DA6EA07-E1B3-4466-A91C-0BF93D31A01F") };
            HierarchyPermissions hp29 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3") && x.GranularPermissionId == Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC")), SecurableClassId = Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3"), GranularPermissionId = Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD") && x.GranularPermissionId == Guid.Parse("B6866B94-4AE1-457C-9ABA-167A7794ADBA")), ParentSecurableClassId = Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD"), ParentGranularPermissionId = Guid.Parse("B6866B94-4AE1-457C-9ABA-167A7794ADBA"), Id = Guid.Parse("F9F07AC3-C746-416F-9481-FBFE6E2AA266") };
            HierarchyPermissions hp30 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3") && x.GranularPermissionId == Guid.Parse("8F3E85F6-4805-4A19-A430-6182930E6FDE")), SecurableClassId = Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3"), GranularPermissionId = Guid.Parse("8F3E85F6-4805-4A19-A430-6182930E6FDE"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3") && x.GranularPermissionId == Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC")), ParentSecurableClassId = Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3"), ParentGranularPermissionId = Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC"), Id = Guid.Parse("F2AED841-3C18-4F53-8C76-4800F26941F1") };
            HierarchyPermissions hp31 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3") && x.GranularPermissionId == Guid.Parse("8F3E85F6-4805-4A19-A430-6182930E6FDE")), SecurableClassId = Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3"), GranularPermissionId = Guid.Parse("8F3E85F6-4805-4A19-A430-6182930E6FDE"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3") && x.GranularPermissionId == Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88")), ParentSecurableClassId = Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3"), ParentGranularPermissionId = Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88"), Id = Guid.Parse("8EEF5D94-3B51-4F1E-BF51-678811C8C28B") };
            HierarchyPermissions hp32 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3") && x.GranularPermissionId == Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88")), SecurableClassId = Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3"), GranularPermissionId = Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD") && x.GranularPermissionId == Guid.Parse("CB6D08BD-B510-4405-A193-8D3D85575618")), ParentSecurableClassId = Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD"), ParentGranularPermissionId = Guid.Parse("CB6D08BD-B510-4405-A193-8D3D85575618"), Id = Guid.Parse("A49E9D11-426B-47BE-9589-23BADAC792CC") };
            HierarchyPermissions hp33 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3") && x.GranularPermissionId == Guid.Parse("EE1EF478-C858-4505-8422-6827FFD86DA0")), SecurableClassId = Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3"), GranularPermissionId = Guid.Parse("EE1EF478-C858-4505-8422-6827FFD86DA0"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3") && x.GranularPermissionId == Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC")), ParentSecurableClassId = Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3"), ParentGranularPermissionId = Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC"), Id = Guid.Parse("E62499B8-8147-4017-AE66-5A2F977F7EEA") };
            HierarchyPermissions hp34 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3") && x.GranularPermissionId == Guid.Parse("7702405E-4AAC-409E-9780-6DF5982170AD")), SecurableClassId = Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3"), GranularPermissionId = Guid.Parse("7702405E-4AAC-409E-9780-6DF5982170AD"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3") && x.GranularPermissionId == Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC")), ParentSecurableClassId = Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3"), ParentGranularPermissionId = Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC"), Id = Guid.Parse("9E74648E-186A-4C5A-A95C-FBF4B48A0EF8") };
            HierarchyPermissions hp35 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3") && x.GranularPermissionId == Guid.Parse("3ED98090-4894-4E93-933E-A4D463BBCE53")), SecurableClassId = Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3"), GranularPermissionId = Guid.Parse("3ED98090-4894-4E93-933E-A4D463BBCE53"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3") && x.GranularPermissionId == Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC")), ParentSecurableClassId = Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3"), ParentGranularPermissionId = Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC"), Id = Guid.Parse("8204A7C9-20E5-4CB8-944B-58A6B437AB4B") };
            HierarchyPermissions hp36 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3") && x.GranularPermissionId == Guid.Parse("E50A90EA-E6AF-454E-9E7E-B6427B61F950")), SecurableClassId = Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3"), GranularPermissionId = Guid.Parse("E50A90EA-E6AF-454E-9E7E-B6427B61F950"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD") && x.GranularPermissionId == Guid.Parse("6DD33BD4-CF0F-452B-8532-DFE1AFBB7E15")), ParentSecurableClassId = Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD"), ParentGranularPermissionId = Guid.Parse("6DD33BD4-CF0F-452B-8532-DFE1AFBB7E15"), Id = Guid.Parse("558BFABF-96FD-421D-B421-C9081576576E") };
            HierarchyPermissions hp37 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3") && x.GranularPermissionId == Guid.Parse("96FFAEDA-F2E9-4056-B394-C63D56DF38B5")), SecurableClassId = Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3"), GranularPermissionId = Guid.Parse("96FFAEDA-F2E9-4056-B394-C63D56DF38B5"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3") && x.GranularPermissionId == Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88")), ParentSecurableClassId = Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3"), ParentGranularPermissionId = Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88"), Id = Guid.Parse("8FCA71BB-EB33-4150-9A19-B0A0118C4E89") };
            HierarchyPermissions hp38 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3") && x.GranularPermissionId == Guid.Parse("4F7BDC87-959A-41A6-BA02-CB84B4C56E65")), SecurableClassId = Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3"), GranularPermissionId = Guid.Parse("4F7BDC87-959A-41A6-BA02-CB84B4C56E65"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3") && x.GranularPermissionId == Guid.Parse("7702405E-4AAC-409E-9780-6DF5982170AD")), ParentSecurableClassId = Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3"), ParentGranularPermissionId = Guid.Parse("7702405E-4AAC-409E-9780-6DF5982170AD"), Id = Guid.Parse("81F51AA1-972E-4566-A8AB-9346709FE536") };
            HierarchyPermissions hp39 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3") && x.GranularPermissionId == Guid.Parse("0686D31E-3C46-4109-A1EC-D0EF8CA00653")), SecurableClassId = Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3"), GranularPermissionId = Guid.Parse("0686D31E-3C46-4109-A1EC-D0EF8CA00653"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3") && x.GranularPermissionId == Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC")), ParentSecurableClassId = Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3"), ParentGranularPermissionId = Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC"), Id = Guid.Parse("361A9977-D325-40BB-8054-C4A0CB093664") };
            HierarchyPermissions hp40 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3") && x.GranularPermissionId == Guid.Parse("A2DDA07F-25CD-4652-BC71-FA26F2EF5F59")), SecurableClassId = Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3"), GranularPermissionId = Guid.Parse("A2DDA07F-25CD-4652-BC71-FA26F2EF5F59"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD") && x.GranularPermissionId == Guid.Parse("D3F76511-60E2-4303-8D66-41DEE30A619F")), ParentSecurableClassId = Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD"), ParentGranularPermissionId = Guid.Parse("D3F76511-60E2-4303-8D66-41DEE30A619F"), Id = Guid.Parse("55B94DC0-2EBA-4093-95D9-0BD454B0873D") };
            HierarchyPermissions hp41 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3") && x.GranularPermissionId == Guid.Parse("DB81C34E-4DF5-4A4D-9BAD-FB1DDE10743A")), SecurableClassId = Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3"), GranularPermissionId = Guid.Parse("DB81C34E-4DF5-4A4D-9BAD-FB1DDE10743A"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3") && x.GranularPermissionId == Guid.Parse("8F3E85F6-4805-4A19-A430-6182930E6FDE")), ParentSecurableClassId = Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3"), ParentGranularPermissionId = Guid.Parse("8F3E85F6-4805-4A19-A430-6182930E6FDE"), Id = Guid.Parse("CD863A01-7316-4A56-A8E4-B9FD34245151") };
            HierarchyPermissions hp42 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD") && x.GranularPermissionId == Guid.Parse("B6866B94-4AE1-457C-9ABA-167A7794ADBA")), SecurableClassId = Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD"), GranularPermissionId = Guid.Parse("B6866B94-4AE1-457C-9ABA-167A7794ADBA"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD") && x.GranularPermissionId == Guid.Parse("CB6D08BD-B510-4405-A193-8D3D85575618")), ParentSecurableClassId = Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD"), ParentGranularPermissionId = Guid.Parse("CB6D08BD-B510-4405-A193-8D3D85575618"), Id = Guid.Parse("1B785F21-CB3D-41D8-B427-49CBD24A70BB") };
            HierarchyPermissions hp43 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD") && x.GranularPermissionId == Guid.Parse("159F7116-A56C-4507-9FD2-27FA2DC26E87")), SecurableClassId = Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD"), GranularPermissionId = Guid.Parse("159F7116-A56C-4507-9FD2-27FA2DC26E87"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD") && x.GranularPermissionId == Guid.Parse("CB6D08BD-B510-4405-A193-8D3D85575618")), ParentSecurableClassId = Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD"), ParentGranularPermissionId = Guid.Parse("CB6D08BD-B510-4405-A193-8D3D85575618"), Id = Guid.Parse("E9EA7939-2F6D-4C6F-9CAC-2A0C5ECCD6AD") };
            HierarchyPermissions hp44 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD") && x.GranularPermissionId == Guid.Parse("D3F76511-60E2-4303-8D66-41DEE30A619F")), SecurableClassId = Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD"), GranularPermissionId = Guid.Parse("D3F76511-60E2-4303-8D66-41DEE30A619F"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD") && x.GranularPermissionId == Guid.Parse("B6866B94-4AE1-457C-9ABA-167A7794ADBA")), ParentSecurableClassId = Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD"), ParentGranularPermissionId = Guid.Parse("B6866B94-4AE1-457C-9ABA-167A7794ADBA"), Id = Guid.Parse("0AE89CC0-4E42-4E44-B54F-D6E6D0C737A1") };
            HierarchyPermissions hp45 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD") && x.GranularPermissionId == Guid.Parse("3E9F374C-057A-43FB-A10E-44960AE07978")), SecurableClassId = Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD"), GranularPermissionId = Guid.Parse("3E9F374C-057A-43FB-A10E-44960AE07978"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD") && x.GranularPermissionId == Guid.Parse("CB6D08BD-B510-4405-A193-8D3D85575618")), ParentSecurableClassId = Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD"), ParentGranularPermissionId = Guid.Parse("CB6D08BD-B510-4405-A193-8D3D85575618"), Id = Guid.Parse("68532BA9-0EF2-4A1A-95B4-A25E41671541") };
            HierarchyPermissions hp46 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD") && x.GranularPermissionId == Guid.Parse("CB6D08BD-B510-4405-A193-8D3D85575618")), SecurableClassId = Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD"), GranularPermissionId = Guid.Parse("CB6D08BD-B510-4405-A193-8D3D85575618"), ParentPermission = null, ParentSecurableClassId = null, ParentGranularPermissionId = null, Id = Guid.Parse("750CA72A-2149-4E38-8B52-5EB2286FDD99") };
            HierarchyPermissions hp47 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD") && x.GranularPermissionId == Guid.Parse("7B9116C8-76F6-4002-8910-9892A4BADFB8")), SecurableClassId = Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD"), GranularPermissionId = Guid.Parse("7B9116C8-76F6-4002-8910-9892A4BADFB8"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD") && x.GranularPermissionId == Guid.Parse("CB6D08BD-B510-4405-A193-8D3D85575618")), ParentSecurableClassId = Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD"), ParentGranularPermissionId = Guid.Parse("CB6D08BD-B510-4405-A193-8D3D85575618"), Id = Guid.Parse("F40FE6F0-E2E0-4F72-8421-F58800E52F19") };
            HierarchyPermissions hp48 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD") && x.GranularPermissionId == Guid.Parse("BEF7C3F8-B258-4876-B657-BA78149B52E3")), SecurableClassId = Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD"), GranularPermissionId = Guid.Parse("BEF7C3F8-B258-4876-B657-BA78149B52E3"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD") && x.GranularPermissionId == Guid.Parse("CB6D08BD-B510-4405-A193-8D3D85575618")), ParentSecurableClassId = Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD"), ParentGranularPermissionId = Guid.Parse("CB6D08BD-B510-4405-A193-8D3D85575618"), Id = Guid.Parse("225DD4B7-7F4B-4CCE-8DD4-3D53757ECC7E") };
            HierarchyPermissions hp49 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD") && x.GranularPermissionId == Guid.Parse("1F1CCA6D-6D37-41FB-ACF0-C222C6C275E8")), SecurableClassId = Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD"), GranularPermissionId = Guid.Parse("1F1CCA6D-6D37-41FB-ACF0-C222C6C275E8"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD") && x.GranularPermissionId == Guid.Parse("71589504-7CA0-4859-8F91-F9A0A3A9D3D4")), ParentSecurableClassId = Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD"), ParentGranularPermissionId = Guid.Parse("71589504-7CA0-4859-8F91-F9A0A3A9D3D4"), Id = Guid.Parse("E0B3B064-C937-4041-AAD1-20D7265D555D") };
            HierarchyPermissions hp50 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD") && x.GranularPermissionId == Guid.Parse("0FDB9437-AF60-466D-A509-C28995C7525C")), SecurableClassId = Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD"), GranularPermissionId = Guid.Parse("0FDB9437-AF60-466D-A509-C28995C7525C"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD") && x.GranularPermissionId == Guid.Parse("CB6D08BD-B510-4405-A193-8D3D85575618")), ParentSecurableClassId = Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD"), ParentGranularPermissionId = Guid.Parse("CB6D08BD-B510-4405-A193-8D3D85575618"), Id = Guid.Parse("39E4B518-A7EF-4C11-BBC0-79AE20A66CE2") };
            HierarchyPermissions hp51 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD") && x.GranularPermissionId == Guid.Parse("6DD33BD4-CF0F-452B-8532-DFE1AFBB7E15")), SecurableClassId = Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD"), GranularPermissionId = Guid.Parse("6DD33BD4-CF0F-452B-8532-DFE1AFBB7E15"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD") && x.GranularPermissionId == Guid.Parse("CB6D08BD-B510-4405-A193-8D3D85575618")), ParentSecurableClassId = Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD"), ParentGranularPermissionId = Guid.Parse("CB6D08BD-B510-4405-A193-8D3D85575618"), Id = Guid.Parse("01325054-2099-46DE-ABEE-E6C5B7D237F0") };
            HierarchyPermissions hp52 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD") && x.GranularPermissionId == Guid.Parse("71589504-7CA0-4859-8F91-F9A0A3A9D3D4")), SecurableClassId = Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD"), GranularPermissionId = Guid.Parse("71589504-7CA0-4859-8F91-F9A0A3A9D3D4"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD") && x.GranularPermissionId == Guid.Parse("CB6D08BD-B510-4405-A193-8D3D85575618")), ParentSecurableClassId = Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD"), ParentGranularPermissionId = Guid.Parse("CB6D08BD-B510-4405-A193-8D3D85575618"), Id = Guid.Parse("1A419B41-E523-461B-B3F6-77219EDB3E0A") };
            HierarchyPermissions hp53 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("C6760E1C-03E3-4FCE-ADD2-9C112D8F222A") && x.GranularPermissionId == Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659")), SecurableClassId = Guid.Parse("C6760E1C-03E3-4FCE-ADD2-9C112D8F222A"), GranularPermissionId = Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3") && x.GranularPermissionId == Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659")), ParentSecurableClassId = Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3"), ParentGranularPermissionId = Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659"), Id = Guid.Parse("A08A8640-E468-4786-9BD6-B37DFB679DB8") };
            HierarchyPermissions hp54 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("C6760E1C-03E3-4FCE-ADD2-9C112D8F222A") && x.GranularPermissionId == Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659")), SecurableClassId = Guid.Parse("C6760E1C-03E3-4FCE-ADD2-9C112D8F222A"), GranularPermissionId = Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("C6760E1C-03E3-4FCE-ADD2-9C112D8F222A") && x.GranularPermissionId == Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88")), ParentSecurableClassId = Guid.Parse("C6760E1C-03E3-4FCE-ADD2-9C112D8F222A"), ParentGranularPermissionId = Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88"), Id = Guid.Parse("4AF263CD-D37C-4D4D-894A-EA9F0BC59EAA") };
            HierarchyPermissions hp55 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("C6760E1C-03E3-4FCE-ADD2-9C112D8F222A") && x.GranularPermissionId == Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC")), SecurableClassId = Guid.Parse("C6760E1C-03E3-4FCE-ADD2-9C112D8F222A"), GranularPermissionId = Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3") && x.GranularPermissionId == Guid.Parse("8F3E85F6-4805-4A19-A430-6182930E6FDE")), ParentSecurableClassId = Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3"), ParentGranularPermissionId = Guid.Parse("8F3E85F6-4805-4A19-A430-6182930E6FDE"), Id = Guid.Parse("D829B3CE-A194-4AF4-843A-5D8E0C80ABA7") };
            HierarchyPermissions hp56 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("C6760E1C-03E3-4FCE-ADD2-9C112D8F222A") && x.GranularPermissionId == Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC")), SecurableClassId = Guid.Parse("C6760E1C-03E3-4FCE-ADD2-9C112D8F222A"), GranularPermissionId = Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("C6760E1C-03E3-4FCE-ADD2-9C112D8F222A") && x.GranularPermissionId == Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88")), ParentSecurableClassId = Guid.Parse("C6760E1C-03E3-4FCE-ADD2-9C112D8F222A"), ParentGranularPermissionId = Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88"), Id = Guid.Parse("A9D6A2DF-B928-48A3-B072-B69B79DF747A") };
            HierarchyPermissions hp57 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("C6760E1C-03E3-4FCE-ADD2-9C112D8F222A") && x.GranularPermissionId == Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88")), SecurableClassId = Guid.Parse("C6760E1C-03E3-4FCE-ADD2-9C112D8F222A"), GranularPermissionId = Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3") && x.GranularPermissionId == Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88")), ParentSecurableClassId = Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3"), ParentGranularPermissionId = Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88"), Id = Guid.Parse("438709D4-1151-49E2-8E18-9547C16F2F43") };
            HierarchyPermissions hp58 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("C6760E1C-03E3-4FCE-ADD2-9C112D8F222A") && x.GranularPermissionId == Guid.Parse("96FFAEDA-F2E9-4056-B394-C63D56DF38B5")), SecurableClassId = Guid.Parse("C6760E1C-03E3-4FCE-ADD2-9C112D8F222A"), GranularPermissionId = Guid.Parse("96FFAEDA-F2E9-4056-B394-C63D56DF38B5"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("C6760E1C-03E3-4FCE-ADD2-9C112D8F222A") && x.GranularPermissionId == Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88")), ParentSecurableClassId = Guid.Parse("C6760E1C-03E3-4FCE-ADD2-9C112D8F222A"), ParentGranularPermissionId = Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88"), Id = Guid.Parse("0185BD97-F8B4-4420-8F7F-4D607DF743C0") };
            HierarchyPermissions hp59 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("F02FB277-7050-465C-BC9E-C90CDF6DE9DB") && x.GranularPermissionId == Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659")), SecurableClassId = Guid.Parse("F02FB277-7050-465C-BC9E-C90CDF6DE9DB"), GranularPermissionId = Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD") && x.GranularPermissionId == Guid.Parse("159F7116-A56C-4507-9FD2-27FA2DC26E87")), ParentSecurableClassId = Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD"), ParentGranularPermissionId = Guid.Parse("159F7116-A56C-4507-9FD2-27FA2DC26E87"), Id = Guid.Parse("C21E7145-7E32-4718-BAD4-A6DF51AD21D4") };
            HierarchyPermissions hp60 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("F02FB277-7050-465C-BC9E-C90CDF6DE9DB") && x.GranularPermissionId == Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659")), SecurableClassId = Guid.Parse("F02FB277-7050-465C-BC9E-C90CDF6DE9DB"), GranularPermissionId = Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("F02FB277-7050-465C-BC9E-C90CDF6DE9DB") && x.GranularPermissionId == Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88")), ParentSecurableClassId = Guid.Parse("F02FB277-7050-465C-BC9E-C90CDF6DE9DB"), ParentGranularPermissionId = Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88"), Id = Guid.Parse("1EC2118A-5F9D-46A0-AAF4-7651DAB49536") };
            HierarchyPermissions hp61 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("F02FB277-7050-465C-BC9E-C90CDF6DE9DB") && x.GranularPermissionId == Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC")), SecurableClassId = Guid.Parse("F02FB277-7050-465C-BC9E-C90CDF6DE9DB"), GranularPermissionId = Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD") && x.GranularPermissionId == Guid.Parse("BEF7C3F8-B258-4876-B657-BA78149B52E3")), ParentSecurableClassId = Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD"), ParentGranularPermissionId = Guid.Parse("BEF7C3F8-B258-4876-B657-BA78149B52E3"), Id = Guid.Parse("BBB1E680-85EE-4985-A0D3-7C5DA04CC7F6") };
            HierarchyPermissions hp62 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("F02FB277-7050-465C-BC9E-C90CDF6DE9DB") && x.GranularPermissionId == Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC")), SecurableClassId = Guid.Parse("F02FB277-7050-465C-BC9E-C90CDF6DE9DB"), GranularPermissionId = Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("F02FB277-7050-465C-BC9E-C90CDF6DE9DB") && x.GranularPermissionId == Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88")), ParentSecurableClassId = Guid.Parse("F02FB277-7050-465C-BC9E-C90CDF6DE9DB"), ParentGranularPermissionId = Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88"), Id = Guid.Parse("BA52F800-2F82-418D-829F-B056C71DEB14") };
            HierarchyPermissions hp63 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("F02FB277-7050-465C-BC9E-C90CDF6DE9DB") && x.GranularPermissionId == Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88")), SecurableClassId = Guid.Parse("F02FB277-7050-465C-BC9E-C90CDF6DE9DB"), GranularPermissionId = Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD") && x.GranularPermissionId == Guid.Parse("CB6D08BD-B510-4405-A193-8D3D85575618")), ParentSecurableClassId = Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD"), ParentGranularPermissionId = Guid.Parse("CB6D08BD-B510-4405-A193-8D3D85575618"), Id = Guid.Parse("0A56CFA3-309F-482C-B510-803BC3A95BDB") };
            HierarchyPermissions hp64 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("F77D03D1-6506-419D-AA55-E2C1D54625CA") && x.GranularPermissionId == Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659")), SecurableClassId = Guid.Parse("F77D03D1-6506-419D-AA55-E2C1D54625CA"), GranularPermissionId = Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD") && x.GranularPermissionId == Guid.Parse("159F7116-A56C-4507-9FD2-27FA2DC26E87")), ParentSecurableClassId = Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD"), ParentGranularPermissionId = Guid.Parse("159F7116-A56C-4507-9FD2-27FA2DC26E87"), Id = Guid.Parse("5182CFEA-9DBC-4C9F-8323-B20B3371623C") };
            HierarchyPermissions hp65 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("F77D03D1-6506-419D-AA55-E2C1D54625CA") && x.GranularPermissionId == Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659")), SecurableClassId = Guid.Parse("F77D03D1-6506-419D-AA55-E2C1D54625CA"), GranularPermissionId = Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("F77D03D1-6506-419D-AA55-E2C1D54625CA") && x.GranularPermissionId == Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88")), ParentSecurableClassId = Guid.Parse("F77D03D1-6506-419D-AA55-E2C1D54625CA"), ParentGranularPermissionId = Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88"), Id = Guid.Parse("60C5A279-2765-4112-A290-698404DC31F4") };
            HierarchyPermissions hp66 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("F77D03D1-6506-419D-AA55-E2C1D54625CA") && x.GranularPermissionId == Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88")), SecurableClassId = Guid.Parse("F77D03D1-6506-419D-AA55-E2C1D54625CA"), GranularPermissionId = Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD") && x.GranularPermissionId == Guid.Parse("CB6D08BD-B510-4405-A193-8D3D85575618")), ParentSecurableClassId = Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD"), ParentGranularPermissionId = Guid.Parse("CB6D08BD-B510-4405-A193-8D3D85575618"), Id = Guid.Parse("BB5C4094-A34C-4720-B1B8-E1E07E397514") };
            HierarchyPermissions hp67 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("ACCC3A64-56B6-4821-AB47-EC501F76ABD8") && x.GranularPermissionId == Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659")), SecurableClassId = Guid.Parse("ACCC3A64-56B6-4821-AB47-EC501F76ABD8"), GranularPermissionId = Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("C6760E1C-03E3-4FCE-ADD2-9C112D8F222A") && x.GranularPermissionId == Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659")), ParentSecurableClassId = Guid.Parse("C6760E1C-03E3-4FCE-ADD2-9C112D8F222A"), ParentGranularPermissionId = Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659"), Id = Guid.Parse("5E703571-0282-4EB9-87EE-DA1CAB1B0F8F") };
            HierarchyPermissions hp68 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("ACCC3A64-56B6-4821-AB47-EC501F76ABD8") && x.GranularPermissionId == Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659")), SecurableClassId = Guid.Parse("ACCC3A64-56B6-4821-AB47-EC501F76ABD8"), GranularPermissionId = Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("ACCC3A64-56B6-4821-AB47-EC501F76ABD8") && x.GranularPermissionId == Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88")), ParentSecurableClassId = Guid.Parse("ACCC3A64-56B6-4821-AB47-EC501F76ABD8"), ParentGranularPermissionId = Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88"), Id = Guid.Parse("3BF0F54A-D0B7-4C9C-B529-FF54B5DEFF4B") };
            HierarchyPermissions hp69 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("ACCC3A64-56B6-4821-AB47-EC501F76ABD8") && x.GranularPermissionId == Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC")), SecurableClassId = Guid.Parse("ACCC3A64-56B6-4821-AB47-EC501F76ABD8"), GranularPermissionId = Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("C6760E1C-03E3-4FCE-ADD2-9C112D8F222A") && x.GranularPermissionId == Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC")), ParentSecurableClassId = Guid.Parse("C6760E1C-03E3-4FCE-ADD2-9C112D8F222A"), ParentGranularPermissionId = Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC"), Id = Guid.Parse("6ECE1CA7-D977-4B59-8E1B-ABD5DF127336") };
            HierarchyPermissions hp70 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("ACCC3A64-56B6-4821-AB47-EC501F76ABD8") && x.GranularPermissionId == Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC")), SecurableClassId = Guid.Parse("ACCC3A64-56B6-4821-AB47-EC501F76ABD8"), GranularPermissionId = Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("ACCC3A64-56B6-4821-AB47-EC501F76ABD8") && x.GranularPermissionId == Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88")), ParentSecurableClassId = Guid.Parse("ACCC3A64-56B6-4821-AB47-EC501F76ABD8"), ParentGranularPermissionId = Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88"), Id = Guid.Parse("80582672-CB3B-4C5E-B932-045A7D705912") };
            HierarchyPermissions hp71 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("ACCC3A64-56B6-4821-AB47-EC501F76ABD8") && x.GranularPermissionId == Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88")), SecurableClassId = Guid.Parse("ACCC3A64-56B6-4821-AB47-EC501F76ABD8"), GranularPermissionId = Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("C6760E1C-03E3-4FCE-ADD2-9C112D8F222A") && x.GranularPermissionId == Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88")), ParentSecurableClassId = Guid.Parse("C6760E1C-03E3-4FCE-ADD2-9C112D8F222A"), ParentGranularPermissionId = Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88"), Id = Guid.Parse("9AD1CAB8-41B6-488B-B9B2-0CB38865E589") };
            HierarchyPermissions hp72 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("ACCC3A64-56B6-4821-AB47-EC501F76ABD8") && x.GranularPermissionId == Guid.Parse("96FFAEDA-F2E9-4056-B394-C63D56DF38B5")), SecurableClassId = Guid.Parse("ACCC3A64-56B6-4821-AB47-EC501F76ABD8"), GranularPermissionId = Guid.Parse("96FFAEDA-F2E9-4056-B394-C63D56DF38B5"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("ACCC3A64-56B6-4821-AB47-EC501F76ABD8") && x.GranularPermissionId == Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88")), ParentSecurableClassId = Guid.Parse("ACCC3A64-56B6-4821-AB47-EC501F76ABD8"), ParentGranularPermissionId = Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88"), Id = Guid.Parse("792FB12F-08B5-4712-8FD0-B683D54190C6") };
            HierarchyPermissions hp73 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("5E3520C3-3204-4763-9B44-F84C354369B8") && x.GranularPermissionId == Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659")), SecurableClassId = Guid.Parse("5E3520C3-3204-4763-9B44-F84C354369B8"), GranularPermissionId = Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3") && x.GranularPermissionId == Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659")), ParentSecurableClassId = Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3"), ParentGranularPermissionId = Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659"), Id = Guid.Parse("CC76B7E9-1679-49BB-A735-B800282F7E89") };
            HierarchyPermissions hp74 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("5E3520C3-3204-4763-9B44-F84C354369B8") && x.GranularPermissionId == Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659")), SecurableClassId = Guid.Parse("5E3520C3-3204-4763-9B44-F84C354369B8"), GranularPermissionId = Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("5E3520C3-3204-4763-9B44-F84C354369B8") && x.GranularPermissionId == Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88")), ParentSecurableClassId = Guid.Parse("5E3520C3-3204-4763-9B44-F84C354369B8"), ParentGranularPermissionId = Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88"), Id = Guid.Parse("A6BC8367-740A-4E06-8AB6-74E7190996EE") };
            HierarchyPermissions hp75 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("5E3520C3-3204-4763-9B44-F84C354369B8") && x.GranularPermissionId == Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC")), SecurableClassId = Guid.Parse("5E3520C3-3204-4763-9B44-F84C354369B8"), GranularPermissionId = Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3") && x.GranularPermissionId == Guid.Parse("7702405E-4AAC-409E-9780-6DF5982170AD")), ParentSecurableClassId = Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3"), ParentGranularPermissionId = Guid.Parse("7702405E-4AAC-409E-9780-6DF5982170AD"), Id = Guid.Parse("D50BA33F-1E0C-482C-99B5-40FE16421A09") };
            HierarchyPermissions hp76 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("5E3520C3-3204-4763-9B44-F84C354369B8") && x.GranularPermissionId == Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC")), SecurableClassId = Guid.Parse("5E3520C3-3204-4763-9B44-F84C354369B8"), GranularPermissionId = Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("5E3520C3-3204-4763-9B44-F84C354369B8") && x.GranularPermissionId == Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88")), ParentSecurableClassId = Guid.Parse("5E3520C3-3204-4763-9B44-F84C354369B8"), ParentGranularPermissionId = Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88"), Id = Guid.Parse("87C84968-A8A3-46AE-BCB9-364A3F13D28C") };
            HierarchyPermissions hp77 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("5E3520C3-3204-4763-9B44-F84C354369B8") && x.GranularPermissionId == Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88")), SecurableClassId = Guid.Parse("5E3520C3-3204-4763-9B44-F84C354369B8"), GranularPermissionId = Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3") && x.GranularPermissionId == Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88")), ParentSecurableClassId = Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3"), ParentGranularPermissionId = Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88"), Id = Guid.Parse("6330D88C-E328-4C0B-96CD-71E34004CF1B") };
            HierarchyPermissions hp78 = new HierarchyPermissions { Permission = permissions.Single(x => x.SecurableClassId == Guid.Parse("5E3520C3-3204-4763-9B44-F84C354369B8") && x.GranularPermissionId == Guid.Parse("96FFAEDA-F2E9-4056-B394-C63D56DF38B5")), SecurableClassId = Guid.Parse("5E3520C3-3204-4763-9B44-F84C354369B8"), GranularPermissionId = Guid.Parse("96FFAEDA-F2E9-4056-B394-C63D56DF38B5"), ParentPermission = permissions.Single(x => x.SecurableClassId == Guid.Parse("5E3520C3-3204-4763-9B44-F84C354369B8") && x.GranularPermissionId == Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88")), ParentSecurableClassId = Guid.Parse("5E3520C3-3204-4763-9B44-F84C354369B8"), ParentGranularPermissionId = Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88"), Id = Guid.Parse("2EC97124-FD2A-464B-8B05-F4B9AFA74559") };

            List<HierarchyPermissions> hierarchyPermissions = new List<HierarchyPermissions>
            {
                hp1
                ,hp2
                ,hp3
                ,hp4
                ,hp5
                ,hp6
                ,hp7
                ,hp8
                ,hp9
                ,hp10
                ,hp11
                ,hp12
                ,hp13
                ,hp14
                ,hp15
                ,hp16
                ,hp17
                ,hp18
                ,hp19
                ,hp20
                ,hp21
                ,hp22
                ,hp23
                ,hp24
                ,hp25
                ,hp26
                ,hp27
                ,hp28
                ,hp29
                ,hp30
                ,hp31
                ,hp32
                ,hp33
                ,hp34
                ,hp35
                ,hp36
                ,hp37
                ,hp38
                ,hp39
                ,hp40
                ,hp41
                ,hp42
                ,hp43
                ,hp44
                ,hp45
                ,hp46
                ,hp47
                ,hp48
                ,hp49
                ,hp50
                ,hp51
                ,hp52
                ,hp53
                ,hp54
                ,hp55
                ,hp56
                ,hp57
                ,hp58
                ,hp59
                ,hp60
                ,hp61
                ,hp62
                ,hp63
                ,hp64
                ,hp65
                ,hp66
                ,hp67
                ,hp68
                ,hp69
                ,hp70
                ,hp71
                ,hp72
                ,hp73
                ,hp74
                ,hp75
                ,hp76
                ,hp77
                ,hp78
            };

            return hierarchyPermissions;
        }

        private List<PermissionBySecurable> CreatePermissions(List<SecurableClass> securableClasses, List<GranularPermission> granularPermissions)
        {
            PermissionBySecurable pbs1 = new PermissionBySecurable { SecurableClass = securableClasses.Single(x => x.SecurableClassId == Guid.Parse("ED9F18F4-BFDD-4DF6-BBD1-3CF33E294769")), SecurableClassId = Guid.Parse("ED9F18F4-BFDD-4DF6-BBD1-3CF33E294769"), GranularPermission = granularPermissions.Single(x => x.GranularPermissionId == Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659")), GranularPermissionId = Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659") };
            PermissionBySecurable pbs2 = new PermissionBySecurable { SecurableClass = securableClasses.Single(x => x.SecurableClassId == Guid.Parse("ED9F18F4-BFDD-4DF6-BBD1-3CF33E294769")), SecurableClassId = Guid.Parse("ED9F18F4-BFDD-4DF6-BBD1-3CF33E294769"), GranularPermission = granularPermissions.Single(x => x.GranularPermissionId == Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC")), GranularPermissionId = Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC") };
            PermissionBySecurable pbs3 = new PermissionBySecurable { SecurableClass = securableClasses.Single(x => x.SecurableClassId == Guid.Parse("ED9F18F4-BFDD-4DF6-BBD1-3CF33E294769")), SecurableClassId = Guid.Parse("ED9F18F4-BFDD-4DF6-BBD1-3CF33E294769"), GranularPermission = granularPermissions.Single(x => x.GranularPermissionId == Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88")), GranularPermissionId = Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88") };
            PermissionBySecurable pbs4 = new PermissionBySecurable { SecurableClass = securableClasses.Single(x => x.SecurableClassId == Guid.Parse("ED9F18F4-BFDD-4DF6-BBD1-3CF33E294769")), SecurableClassId = Guid.Parse("ED9F18F4-BFDD-4DF6-BBD1-3CF33E294769"), GranularPermission = granularPermissions.Single(x => x.GranularPermissionId == Guid.Parse("E50A90EA-E6AF-454E-9E7E-B6427B61F950")), GranularPermissionId = Guid.Parse("E50A90EA-E6AF-454E-9E7E-B6427B61F950") };
            PermissionBySecurable pbs5 = new PermissionBySecurable { SecurableClass = securableClasses.Single(x => x.SecurableClassId == Guid.Parse("ED9F18F4-BFDD-4DF6-BBD1-3CF33E294769")), SecurableClassId = Guid.Parse("ED9F18F4-BFDD-4DF6-BBD1-3CF33E294769"), GranularPermission = granularPermissions.Single(x => x.GranularPermissionId == Guid.Parse("96FFAEDA-F2E9-4056-B394-C63D56DF38B5")), GranularPermissionId = Guid.Parse("96FFAEDA-F2E9-4056-B394-C63D56DF38B5") };
            PermissionBySecurable pbs6 = new PermissionBySecurable { SecurableClass = securableClasses.Single(x => x.SecurableClassId == Guid.Parse("236A8A3F-C8AC-4047-B79E-4FECF5E691D4")), SecurableClassId = Guid.Parse("236A8A3F-C8AC-4047-B79E-4FECF5E691D4"), GranularPermission = granularPermissions.Single(x => x.GranularPermissionId == Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659")), GranularPermissionId = Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659") };
            PermissionBySecurable pbs7 = new PermissionBySecurable { SecurableClass = securableClasses.Single(x => x.SecurableClassId == Guid.Parse("236A8A3F-C8AC-4047-B79E-4FECF5E691D4")), SecurableClassId = Guid.Parse("236A8A3F-C8AC-4047-B79E-4FECF5E691D4"), GranularPermission = granularPermissions.Single(x => x.GranularPermissionId == Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC")), GranularPermissionId = Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC") };
            PermissionBySecurable pbs8 = new PermissionBySecurable { SecurableClass = securableClasses.Single(x => x.SecurableClassId == Guid.Parse("236A8A3F-C8AC-4047-B79E-4FECF5E691D4")), SecurableClassId = Guid.Parse("236A8A3F-C8AC-4047-B79E-4FECF5E691D4"), GranularPermission = granularPermissions.Single(x => x.GranularPermissionId == Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88")), GranularPermissionId = Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88") };
            PermissionBySecurable pbs9 = new PermissionBySecurable { SecurableClass = securableClasses.Single(x => x.SecurableClassId == Guid.Parse("6FB2683D-544A-4DD7-9077-5A6E45FB9AE5")), SecurableClassId = Guid.Parse("6FB2683D-544A-4DD7-9077-5A6E45FB9AE5"), GranularPermission = granularPermissions.Single(x => x.GranularPermissionId == Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659")), GranularPermissionId = Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659") };
            PermissionBySecurable pbs10 = new PermissionBySecurable { SecurableClass = securableClasses.Single(x => x.SecurableClassId == Guid.Parse("6FB2683D-544A-4DD7-9077-5A6E45FB9AE5")), SecurableClassId = Guid.Parse("6FB2683D-544A-4DD7-9077-5A6E45FB9AE5"), GranularPermission = granularPermissions.Single(x => x.GranularPermissionId == Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC")), GranularPermissionId = Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC") };
            PermissionBySecurable pbs11 = new PermissionBySecurable { SecurableClass = securableClasses.Single(x => x.SecurableClassId == Guid.Parse("6FB2683D-544A-4DD7-9077-5A6E45FB9AE5")), SecurableClassId = Guid.Parse("6FB2683D-544A-4DD7-9077-5A6E45FB9AE5"), GranularPermission = granularPermissions.Single(x => x.GranularPermissionId == Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88")), GranularPermissionId = Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88") };
            PermissionBySecurable pbs12 = new PermissionBySecurable { SecurableClass = securableClasses.Single(x => x.SecurableClassId == Guid.Parse("6FB2683D-544A-4DD7-9077-5A6E45FB9AE5")), SecurableClassId = Guid.Parse("6FB2683D-544A-4DD7-9077-5A6E45FB9AE5"), GranularPermission = granularPermissions.Single(x => x.GranularPermissionId == Guid.Parse("0E46C67A-8145-40D9-A6CF-7C0EED7A1442")), GranularPermissionId = Guid.Parse("0E46C67A-8145-40D9-A6CF-7C0EED7A1442") };
            PermissionBySecurable pbs13 = new PermissionBySecurable { SecurableClass = securableClasses.Single(x => x.SecurableClassId == Guid.Parse("6FB2683D-544A-4DD7-9077-5A6E45FB9AE5")), SecurableClassId = Guid.Parse("6FB2683D-544A-4DD7-9077-5A6E45FB9AE5"), GranularPermission = granularPermissions.Single(x => x.GranularPermissionId == Guid.Parse("C41262CD-C9C2-492C-8FC0-8F2E031CC99F")), GranularPermissionId = Guid.Parse("C41262CD-C9C2-492C-8FC0-8F2E031CC99F") };
            PermissionBySecurable pbs14 = new PermissionBySecurable { SecurableClass = securableClasses.Single(x => x.SecurableClassId == Guid.Parse("6FB2683D-544A-4DD7-9077-5A6E45FB9AE5")), SecurableClassId = Guid.Parse("6FB2683D-544A-4DD7-9077-5A6E45FB9AE5"), GranularPermission = granularPermissions.Single(x => x.GranularPermissionId == Guid.Parse("96FFAEDA-F2E9-4056-B394-C63D56DF38B5")), GranularPermissionId = Guid.Parse("96FFAEDA-F2E9-4056-B394-C63D56DF38B5") };
            PermissionBySecurable pbs15 = new PermissionBySecurable { SecurableClass = securableClasses.Single(x => x.SecurableClassId == Guid.Parse("E4F6BC86-4909-41A6-BA68-632DBEF8D6D7")), SecurableClassId = Guid.Parse("E4F6BC86-4909-41A6-BA68-632DBEF8D6D7"), GranularPermission = granularPermissions.Single(x => x.GranularPermissionId == Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659")), GranularPermissionId = Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659") };
            PermissionBySecurable pbs16 = new PermissionBySecurable { SecurableClass = securableClasses.Single(x => x.SecurableClassId == Guid.Parse("E4F6BC86-4909-41A6-BA68-632DBEF8D6D7")), SecurableClassId = Guid.Parse("E4F6BC86-4909-41A6-BA68-632DBEF8D6D7"), GranularPermission = granularPermissions.Single(x => x.GranularPermissionId == Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC")), GranularPermissionId = Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC") };
            PermissionBySecurable pbs17 = new PermissionBySecurable { SecurableClass = securableClasses.Single(x => x.SecurableClassId == Guid.Parse("E4F6BC86-4909-41A6-BA68-632DBEF8D6D7")), SecurableClassId = Guid.Parse("E4F6BC86-4909-41A6-BA68-632DBEF8D6D7"), GranularPermission = granularPermissions.Single(x => x.GranularPermissionId == Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88")), GranularPermissionId = Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88") };
            PermissionBySecurable pbs18 = new PermissionBySecurable { SecurableClass = securableClasses.Single(x => x.SecurableClassId == Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3")), SecurableClassId = Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3"), GranularPermission = granularPermissions.Single(x => x.GranularPermissionId == Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659")), GranularPermissionId = Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659") };
            PermissionBySecurable pbs19 = new PermissionBySecurable { SecurableClass = securableClasses.Single(x => x.SecurableClassId == Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3")), SecurableClassId = Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3"), GranularPermission = granularPermissions.Single(x => x.GranularPermissionId == Guid.Parse("B8092C8F-3110-4A07-B2C4-100FBAA23FD0")), GranularPermissionId = Guid.Parse("B8092C8F-3110-4A07-B2C4-100FBAA23FD0") };
            PermissionBySecurable pbs20 = new PermissionBySecurable { SecurableClass = securableClasses.Single(x => x.SecurableClassId == Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3")), SecurableClassId = Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3"), GranularPermission = granularPermissions.Single(x => x.GranularPermissionId == Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC")), GranularPermissionId = Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC") };
            PermissionBySecurable pbs21 = new PermissionBySecurable { SecurableClass = securableClasses.Single(x => x.SecurableClassId == Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3")), SecurableClassId = Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3"), GranularPermission = granularPermissions.Single(x => x.GranularPermissionId == Guid.Parse("8F3E85F6-4805-4A19-A430-6182930E6FDE")), GranularPermissionId = Guid.Parse("8F3E85F6-4805-4A19-A430-6182930E6FDE") };
            PermissionBySecurable pbs22 = new PermissionBySecurable { SecurableClass = securableClasses.Single(x => x.SecurableClassId == Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3")), SecurableClassId = Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3"), GranularPermission = granularPermissions.Single(x => x.GranularPermissionId == Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88")), GranularPermissionId = Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88") };
            PermissionBySecurable pbs23 = new PermissionBySecurable { SecurableClass = securableClasses.Single(x => x.SecurableClassId == Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3")), SecurableClassId = Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3"), GranularPermission = granularPermissions.Single(x => x.GranularPermissionId == Guid.Parse("EE1EF478-C858-4505-8422-6827FFD86DA0")), GranularPermissionId = Guid.Parse("EE1EF478-C858-4505-8422-6827FFD86DA0") };
            PermissionBySecurable pbs24 = new PermissionBySecurable { SecurableClass = securableClasses.Single(x => x.SecurableClassId == Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3")), SecurableClassId = Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3"), GranularPermission = granularPermissions.Single(x => x.GranularPermissionId == Guid.Parse("7702405E-4AAC-409E-9780-6DF5982170AD")), GranularPermissionId = Guid.Parse("7702405E-4AAC-409E-9780-6DF5982170AD") };
            PermissionBySecurable pbs25 = new PermissionBySecurable { SecurableClass = securableClasses.Single(x => x.SecurableClassId == Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3")), SecurableClassId = Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3"), GranularPermission = granularPermissions.Single(x => x.GranularPermissionId == Guid.Parse("3ED98090-4894-4E93-933E-A4D463BBCE53")), GranularPermissionId = Guid.Parse("3ED98090-4894-4E93-933E-A4D463BBCE53") };
            PermissionBySecurable pbs26 = new PermissionBySecurable { SecurableClass = securableClasses.Single(x => x.SecurableClassId == Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3")), SecurableClassId = Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3"), GranularPermission = granularPermissions.Single(x => x.GranularPermissionId == Guid.Parse("E50A90EA-E6AF-454E-9E7E-B6427B61F950")), GranularPermissionId = Guid.Parse("E50A90EA-E6AF-454E-9E7E-B6427B61F950") };
            PermissionBySecurable pbs27 = new PermissionBySecurable { SecurableClass = securableClasses.Single(x => x.SecurableClassId == Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3")), SecurableClassId = Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3"), GranularPermission = granularPermissions.Single(x => x.GranularPermissionId == Guid.Parse("96FFAEDA-F2E9-4056-B394-C63D56DF38B5")), GranularPermissionId = Guid.Parse("96FFAEDA-F2E9-4056-B394-C63D56DF38B5") };
            PermissionBySecurable pbs28 = new PermissionBySecurable { SecurableClass = securableClasses.Single(x => x.SecurableClassId == Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3")), SecurableClassId = Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3"), GranularPermission = granularPermissions.Single(x => x.GranularPermissionId == Guid.Parse("4F7BDC87-959A-41A6-BA02-CB84B4C56E65")), GranularPermissionId = Guid.Parse("4F7BDC87-959A-41A6-BA02-CB84B4C56E65") };
            PermissionBySecurable pbs29 = new PermissionBySecurable { SecurableClass = securableClasses.Single(x => x.SecurableClassId == Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3")), SecurableClassId = Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3"), GranularPermission = granularPermissions.Single(x => x.GranularPermissionId == Guid.Parse("0686D31E-3C46-4109-A1EC-D0EF8CA00653")), GranularPermissionId = Guid.Parse("0686D31E-3C46-4109-A1EC-D0EF8CA00653") };
            PermissionBySecurable pbs30 = new PermissionBySecurable { SecurableClass = securableClasses.Single(x => x.SecurableClassId == Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3")), SecurableClassId = Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3"), GranularPermission = granularPermissions.Single(x => x.GranularPermissionId == Guid.Parse("A2DDA07F-25CD-4652-BC71-FA26F2EF5F59")), GranularPermissionId = Guid.Parse("A2DDA07F-25CD-4652-BC71-FA26F2EF5F59") };
            PermissionBySecurable pbs31 = new PermissionBySecurable { SecurableClass = securableClasses.Single(x => x.SecurableClassId == Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3")), SecurableClassId = Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3"), GranularPermission = granularPermissions.Single(x => x.GranularPermissionId == Guid.Parse("DB81C34E-4DF5-4A4D-9BAD-FB1DDE10743A")), GranularPermissionId = Guid.Parse("DB81C34E-4DF5-4A4D-9BAD-FB1DDE10743A") };
            PermissionBySecurable pbs32 = new PermissionBySecurable { SecurableClass = securableClasses.Single(x => x.SecurableClassId == Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD")), SecurableClassId = Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD"), GranularPermission = granularPermissions.Single(x => x.GranularPermissionId == Guid.Parse("B6866B94-4AE1-457C-9ABA-167A7794ADBA")), GranularPermissionId = Guid.Parse("B6866B94-4AE1-457C-9ABA-167A7794ADBA") };
            PermissionBySecurable pbs33 = new PermissionBySecurable { SecurableClass = securableClasses.Single(x => x.SecurableClassId == Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD")), SecurableClassId = Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD"), GranularPermission = granularPermissions.Single(x => x.GranularPermissionId == Guid.Parse("159F7116-A56C-4507-9FD2-27FA2DC26E87")), GranularPermissionId = Guid.Parse("159F7116-A56C-4507-9FD2-27FA2DC26E87") };
            PermissionBySecurable pbs34 = new PermissionBySecurable { SecurableClass = securableClasses.Single(x => x.SecurableClassId == Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD")), SecurableClassId = Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD"), GranularPermission = granularPermissions.Single(x => x.GranularPermissionId == Guid.Parse("D3F76511-60E2-4303-8D66-41DEE30A619F")), GranularPermissionId = Guid.Parse("D3F76511-60E2-4303-8D66-41DEE30A619F") };
            PermissionBySecurable pbs35 = new PermissionBySecurable { SecurableClass = securableClasses.Single(x => x.SecurableClassId == Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD")), SecurableClassId = Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD"), GranularPermission = granularPermissions.Single(x => x.GranularPermissionId == Guid.Parse("3E9F374C-057A-43FB-A10E-44960AE07978")), GranularPermissionId = Guid.Parse("3E9F374C-057A-43FB-A10E-44960AE07978") };
            PermissionBySecurable pbs36 = new PermissionBySecurable { SecurableClass = securableClasses.Single(x => x.SecurableClassId == Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD")), SecurableClassId = Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD"), GranularPermission = granularPermissions.Single(x => x.GranularPermissionId == Guid.Parse("CB6D08BD-B510-4405-A193-8D3D85575618")), GranularPermissionId = Guid.Parse("CB6D08BD-B510-4405-A193-8D3D85575618") };
            PermissionBySecurable pbs37 = new PermissionBySecurable { SecurableClass = securableClasses.Single(x => x.SecurableClassId == Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD")), SecurableClassId = Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD"), GranularPermission = granularPermissions.Single(x => x.GranularPermissionId == Guid.Parse("7B9116C8-76F6-4002-8910-9892A4BADFB8")), GranularPermissionId = Guid.Parse("7B9116C8-76F6-4002-8910-9892A4BADFB8") };
            PermissionBySecurable pbs38 = new PermissionBySecurable { SecurableClass = securableClasses.Single(x => x.SecurableClassId == Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD")), SecurableClassId = Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD"), GranularPermission = granularPermissions.Single(x => x.GranularPermissionId == Guid.Parse("BEF7C3F8-B258-4876-B657-BA78149B52E3")), GranularPermissionId = Guid.Parse("BEF7C3F8-B258-4876-B657-BA78149B52E3") };
            PermissionBySecurable pbs39 = new PermissionBySecurable { SecurableClass = securableClasses.Single(x => x.SecurableClassId == Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD")), SecurableClassId = Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD"), GranularPermission = granularPermissions.Single(x => x.GranularPermissionId == Guid.Parse("1F1CCA6D-6D37-41FB-ACF0-C222C6C275E8")), GranularPermissionId = Guid.Parse("1F1CCA6D-6D37-41FB-ACF0-C222C6C275E8") };
            PermissionBySecurable pbs40 = new PermissionBySecurable { SecurableClass = securableClasses.Single(x => x.SecurableClassId == Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD")), SecurableClassId = Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD"), GranularPermission = granularPermissions.Single(x => x.GranularPermissionId == Guid.Parse("0FDB9437-AF60-466D-A509-C28995C7525C")), GranularPermissionId = Guid.Parse("0FDB9437-AF60-466D-A509-C28995C7525C") };
            PermissionBySecurable pbs41 = new PermissionBySecurable { SecurableClass = securableClasses.Single(x => x.SecurableClassId == Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD")), SecurableClassId = Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD"), GranularPermission = granularPermissions.Single(x => x.GranularPermissionId == Guid.Parse("6DD33BD4-CF0F-452B-8532-DFE1AFBB7E15")), GranularPermissionId = Guid.Parse("6DD33BD4-CF0F-452B-8532-DFE1AFBB7E15") };
            PermissionBySecurable pbs42 = new PermissionBySecurable { SecurableClass = securableClasses.Single(x => x.SecurableClassId == Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD")), SecurableClassId = Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD"), GranularPermission = granularPermissions.Single(x => x.GranularPermissionId == Guid.Parse("71589504-7CA0-4859-8F91-F9A0A3A9D3D4")), GranularPermissionId = Guid.Parse("71589504-7CA0-4859-8F91-F9A0A3A9D3D4") };
            PermissionBySecurable pbs43 = new PermissionBySecurable { SecurableClass = securableClasses.Single(x => x.SecurableClassId == Guid.Parse("C6760E1C-03E3-4FCE-ADD2-9C112D8F222A")), SecurableClassId = Guid.Parse("C6760E1C-03E3-4FCE-ADD2-9C112D8F222A"), GranularPermission = granularPermissions.Single(x => x.GranularPermissionId == Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659")), GranularPermissionId = Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659") };
            PermissionBySecurable pbs44 = new PermissionBySecurable { SecurableClass = securableClasses.Single(x => x.SecurableClassId == Guid.Parse("C6760E1C-03E3-4FCE-ADD2-9C112D8F222A")), SecurableClassId = Guid.Parse("C6760E1C-03E3-4FCE-ADD2-9C112D8F222A"), GranularPermission = granularPermissions.Single(x => x.GranularPermissionId == Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC")), GranularPermissionId = Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC") };
            PermissionBySecurable pbs45 = new PermissionBySecurable { SecurableClass = securableClasses.Single(x => x.SecurableClassId == Guid.Parse("C6760E1C-03E3-4FCE-ADD2-9C112D8F222A")), SecurableClassId = Guid.Parse("C6760E1C-03E3-4FCE-ADD2-9C112D8F222A"), GranularPermission = granularPermissions.Single(x => x.GranularPermissionId == Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88")), GranularPermissionId = Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88") };
            PermissionBySecurable pbs46 = new PermissionBySecurable { SecurableClass = securableClasses.Single(x => x.SecurableClassId == Guid.Parse("C6760E1C-03E3-4FCE-ADD2-9C112D8F222A")), SecurableClassId = Guid.Parse("C6760E1C-03E3-4FCE-ADD2-9C112D8F222A"), GranularPermission = granularPermissions.Single(x => x.GranularPermissionId == Guid.Parse("96FFAEDA-F2E9-4056-B394-C63D56DF38B5")), GranularPermissionId = Guid.Parse("96FFAEDA-F2E9-4056-B394-C63D56DF38B5") };
            PermissionBySecurable pbs47 = new PermissionBySecurable { SecurableClass = securableClasses.Single(x => x.SecurableClassId == Guid.Parse("F02FB277-7050-465C-BC9E-C90CDF6DE9DB")), SecurableClassId = Guid.Parse("F02FB277-7050-465C-BC9E-C90CDF6DE9DB"), GranularPermission = granularPermissions.Single(x => x.GranularPermissionId == Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659")), GranularPermissionId = Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659") };
            PermissionBySecurable pbs48 = new PermissionBySecurable { SecurableClass = securableClasses.Single(x => x.SecurableClassId == Guid.Parse("F02FB277-7050-465C-BC9E-C90CDF6DE9DB")), SecurableClassId = Guid.Parse("F02FB277-7050-465C-BC9E-C90CDF6DE9DB"), GranularPermission = granularPermissions.Single(x => x.GranularPermissionId == Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC")), GranularPermissionId = Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC") };
            PermissionBySecurable pbs49 = new PermissionBySecurable { SecurableClass = securableClasses.Single(x => x.SecurableClassId == Guid.Parse("F02FB277-7050-465C-BC9E-C90CDF6DE9DB")), SecurableClassId = Guid.Parse("F02FB277-7050-465C-BC9E-C90CDF6DE9DB"), GranularPermission = granularPermissions.Single(x => x.GranularPermissionId == Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88")), GranularPermissionId = Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88") };
            PermissionBySecurable pbs50 = new PermissionBySecurable { SecurableClass = securableClasses.Single(x => x.SecurableClassId == Guid.Parse("F77D03D1-6506-419D-AA55-E2C1D54625CA")), SecurableClassId = Guid.Parse("F77D03D1-6506-419D-AA55-E2C1D54625CA"), GranularPermission = granularPermissions.Single(x => x.GranularPermissionId == Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659")), GranularPermissionId = Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659") };
            PermissionBySecurable pbs51 = new PermissionBySecurable { SecurableClass = securableClasses.Single(x => x.SecurableClassId == Guid.Parse("F77D03D1-6506-419D-AA55-E2C1D54625CA")), SecurableClassId = Guid.Parse("F77D03D1-6506-419D-AA55-E2C1D54625CA"), GranularPermission = granularPermissions.Single(x => x.GranularPermissionId == Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88")), GranularPermissionId = Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88") };
            PermissionBySecurable pbs52 = new PermissionBySecurable { SecurableClass = securableClasses.Single(x => x.SecurableClassId == Guid.Parse("ACCC3A64-56B6-4821-AB47-EC501F76ABD8")), SecurableClassId = Guid.Parse("ACCC3A64-56B6-4821-AB47-EC501F76ABD8"), GranularPermission = granularPermissions.Single(x => x.GranularPermissionId == Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659")), GranularPermissionId = Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659") };
            PermissionBySecurable pbs53 = new PermissionBySecurable { SecurableClass = securableClasses.Single(x => x.SecurableClassId == Guid.Parse("ACCC3A64-56B6-4821-AB47-EC501F76ABD8")), SecurableClassId = Guid.Parse("ACCC3A64-56B6-4821-AB47-EC501F76ABD8"), GranularPermission = granularPermissions.Single(x => x.GranularPermissionId == Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC")), GranularPermissionId = Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC") };
            PermissionBySecurable pbs54 = new PermissionBySecurable { SecurableClass = securableClasses.Single(x => x.SecurableClassId == Guid.Parse("ACCC3A64-56B6-4821-AB47-EC501F76ABD8")), SecurableClassId = Guid.Parse("ACCC3A64-56B6-4821-AB47-EC501F76ABD8"), GranularPermission = granularPermissions.Single(x => x.GranularPermissionId == Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88")), GranularPermissionId = Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88") };
            PermissionBySecurable pbs55 = new PermissionBySecurable { SecurableClass = securableClasses.Single(x => x.SecurableClassId == Guid.Parse("ACCC3A64-56B6-4821-AB47-EC501F76ABD8")), SecurableClassId = Guid.Parse("ACCC3A64-56B6-4821-AB47-EC501F76ABD8"), GranularPermission = granularPermissions.Single(x => x.GranularPermissionId == Guid.Parse("96FFAEDA-F2E9-4056-B394-C63D56DF38B5")), GranularPermissionId = Guid.Parse("96FFAEDA-F2E9-4056-B394-C63D56DF38B5") };
            PermissionBySecurable pbs56 = new PermissionBySecurable { SecurableClass = securableClasses.Single(x => x.SecurableClassId == Guid.Parse("5E3520C3-3204-4763-9B44-F84C354369B8")), SecurableClassId = Guid.Parse("5E3520C3-3204-4763-9B44-F84C354369B8"), GranularPermission = granularPermissions.Single(x => x.GranularPermissionId == Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659")), GranularPermissionId = Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659") };
            PermissionBySecurable pbs57 = new PermissionBySecurable { SecurableClass = securableClasses.Single(x => x.SecurableClassId == Guid.Parse("5E3520C3-3204-4763-9B44-F84C354369B8")), SecurableClassId = Guid.Parse("5E3520C3-3204-4763-9B44-F84C354369B8"), GranularPermission = granularPermissions.Single(x => x.GranularPermissionId == Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC")), GranularPermissionId = Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC") };
            PermissionBySecurable pbs58 = new PermissionBySecurable { SecurableClass = securableClasses.Single(x => x.SecurableClassId == Guid.Parse("5E3520C3-3204-4763-9B44-F84C354369B8")), SecurableClassId = Guid.Parse("5E3520C3-3204-4763-9B44-F84C354369B8"), GranularPermission = granularPermissions.Single(x => x.GranularPermissionId == Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88")), GranularPermissionId = Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88") };
            PermissionBySecurable pbs59 = new PermissionBySecurable { SecurableClass = securableClasses.Single(x => x.SecurableClassId == Guid.Parse("5E3520C3-3204-4763-9B44-F84C354369B8")), SecurableClassId = Guid.Parse("5E3520C3-3204-4763-9B44-F84C354369B8"), GranularPermission = granularPermissions.Single(x => x.GranularPermissionId == Guid.Parse("96FFAEDA-F2E9-4056-B394-C63D56DF38B5")), GranularPermissionId = Guid.Parse("96FFAEDA-F2E9-4056-B394-C63D56DF38B5") };

            return new List<PermissionBySecurable>
            {
                pbs1
                ,pbs2
                ,pbs3
                ,pbs4
                ,pbs5
                ,pbs6
                ,pbs7
                ,pbs8
                ,pbs9
                ,pbs10
                ,pbs11
                ,pbs12
                ,pbs13
                ,pbs14
                ,pbs15
                ,pbs16
                ,pbs17
                ,pbs18
                ,pbs19
                ,pbs20
                ,pbs21
                ,pbs22
                ,pbs23
                ,pbs24
                ,pbs25
                ,pbs26
                ,pbs27
                ,pbs28
                ,pbs29
                ,pbs30
                ,pbs31
                ,pbs32
                ,pbs33
                ,pbs34
                ,pbs35
                ,pbs36
                ,pbs37
                ,pbs38
                ,pbs39
                ,pbs40
                ,pbs41
                ,pbs42
                ,pbs43
                ,pbs44
                ,pbs45
                ,pbs46
                ,pbs47
                ,pbs48
                ,pbs49
                ,pbs50
                ,pbs51
                ,pbs52
                ,pbs53
                ,pbs54
                ,pbs55
                ,pbs56
                ,pbs57
                ,pbs58
                ,pbs59
            };
        }

        private List<GranularPermission> CreateGranularPermissions()
        {
            GranularPermission gpViewDefinition = new GranularPermission { GranularPermissionId = Guid.Parse("2D9A2FCC-074A-4A90-9347-03F2369EE659"), GranularPermissionName = "view definition" };
            GranularPermission gpAlterAnyUser = new GranularPermission { GranularPermissionId = Guid.Parse("B8092C8F-3110-4A07-B2C4-100FBAA23FD0"), GranularPermissionName = "alter any user" };
            GranularPermission gpAlterAnyDatabase = new GranularPermission { GranularPermissionId = Guid.Parse("B6866B94-4AE1-457C-9ABA-167A7794ADBA"), GranularPermissionName = "alter any database" };
            GranularPermission gpViewAnyDefinition = new GranularPermission { GranularPermissionId = Guid.Parse("159F7116-A56C-4507-9FD2-27FA2DC26E87"), GranularPermissionName = "view any definition" };
            GranularPermission gpAlter = new GranularPermission { GranularPermissionId = Guid.Parse("0B466DE1-86D3-45C8-A5AF-37827BDA0BEC"), GranularPermissionName = "alter" };
            GranularPermission gpCreateAnyDatabase = new GranularPermission { GranularPermissionId = Guid.Parse("D3F76511-60E2-4303-8D66-41DEE30A619F"), GranularPermissionName = "create any database" };
            GranularPermission gpAuthenticateServer = new GranularPermission { GranularPermissionId = Guid.Parse("3E9F374C-057A-43FB-A10E-44960AE07978"), GranularPermissionName = "authenticate server" };
            GranularPermission gpAlterAnySchema = new GranularPermission { GranularPermissionId = Guid.Parse("8F3E85F6-4805-4A19-A430-6182930E6FDE"), GranularPermissionName = "alter any schema" };
            GranularPermission gpControl = new GranularPermission { GranularPermissionId = Guid.Parse("FD6AADF8-8737-4A95-8111-671135FD3D88"), GranularPermissionName = "control" };
            GranularPermission gpCreateStream = new GranularPermission { GranularPermissionId = Guid.Parse("EE1EF478-C858-4505-8422-6827FFD86DA0"), GranularPermissionName = "create stream" };
            GranularPermission gpAlterAnyRole = new GranularPermission { GranularPermissionId = Guid.Parse("7702405E-4AAC-409E-9780-6DF5982170AD"), GranularPermissionName = "alter any role" };
            GranularPermission gpAuthenticate = new GranularPermission { GranularPermissionId = Guid.Parse("8D3CA97B-EA49-4631-88C0-6F9D5936104F"), GranularPermissionName = "authenticate" };
            GranularPermission gpRead = new GranularPermission { GranularPermissionId = Guid.Parse("0E46C67A-8145-40D9-A6CF-7C0EED7A1442"), GranularPermissionName = "read" };
            GranularPermission gpBackupDatabase = new GranularPermission { GranularPermissionId = Guid.Parse("9D87AA10-C182-4461-ADEA-81E178E4F3C7"), GranularPermissionName = "backup database" };
            GranularPermission gpControlServer = new GranularPermission { GranularPermissionId = Guid.Parse("CB6D08BD-B510-4405-A193-8D3D85575618"), GranularPermissionName = "control server" };
            GranularPermission gpWrite = new GranularPermission { GranularPermissionId = Guid.Parse("C41262CD-C9C2-492C-8FC0-8F2E031CC99F"), GranularPermissionName = "write" };
            GranularPermission gpViewAnyDatabase = new GranularPermission { GranularPermissionId = Guid.Parse("7B9116C8-76F6-4002-8910-9892A4BADFB8"), GranularPermissionName = "view any database" };
            GranularPermission gpReferences = new GranularPermission { GranularPermissionId = Guid.Parse("413E333D-EE79-4898-9561-A0D919804148"), GranularPermissionName = "references" };
            GranularPermission gpCreateView = new GranularPermission { GranularPermissionId = Guid.Parse("3ED98090-4894-4E93-933E-A4D463BBCE53"), GranularPermissionName = "create view" };
            GranularPermission gpConnect = new GranularPermission { GranularPermissionId = Guid.Parse("E50A90EA-E6AF-454E-9E7E-B6427B61F950"), GranularPermissionName = "connect" };
            GranularPermission gpCreateTable = new GranularPermission { GranularPermissionId = Guid.Parse("195C53EA-0FCF-44CD-8241-BA2FF05B7AF5"), GranularPermissionName = "create table" };
            GranularPermission gpAlterAnyLogin = new GranularPermission { GranularPermissionId = Guid.Parse("BEF7C3F8-B258-4876-B657-BA78149B52E3"), GranularPermissionName = "alter any login" };
            GranularPermission gpCreateEndpoint = new GranularPermission { GranularPermissionId = Guid.Parse("1F1CCA6D-6D37-41FB-ACF0-C222C6C275E8"), GranularPermissionName = "create endpoint" };
            GranularPermission gpConnectEql = new GranularPermission { GranularPermissionId = Guid.Parse("0FDB9437-AF60-466D-A509-C28995C7525C"), GranularPermissionName = "connect sql" };
            GranularPermission gpTakeOwnership = new GranularPermission { GranularPermissionId = Guid.Parse("96FFAEDA-F2E9-4056-B394-C63D56DF38B5"), GranularPermissionName = "take ownership" };
            GranularPermission gpCreateRole = new GranularPermission { GranularPermissionId = Guid.Parse("4F7BDC87-959A-41A6-BA02-CB84B4C56E65"), GranularPermissionName = "create role" };
            GranularPermission gpCreateSource = new GranularPermission { GranularPermissionId = Guid.Parse("0686D31E-3C46-4109-A1EC-D0EF8CA00653"), GranularPermissionName = "create source" };
            GranularPermission gpConnectAnyDatabase = new GranularPermission { GranularPermissionId = Guid.Parse("6DD33BD4-CF0F-452B-8532-DFE1AFBB7E15"), GranularPermissionName = "connect any database" };
            GranularPermission gpAlterAnyEndpoint = new GranularPermission { GranularPermissionId = Guid.Parse("71589504-7CA0-4859-8F91-F9A0A3A9D3D4"), GranularPermissionName = "alter any endpoint" };
            GranularPermission gpCreateDatabase = new GranularPermission { GranularPermissionId = Guid.Parse("A2DDA07F-25CD-4652-BC71-FA26F2EF5F59"), GranularPermissionName = "create database" };
            GranularPermission gpCreateSchema = new GranularPermission { GranularPermissionId = Guid.Parse("DB81C34E-4DF5-4A4D-9BAD-FB1DDE10743A"), GranularPermissionName = "create schema" };

            return new List<GranularPermission>
            {
                gpViewDefinition
                ,gpAlterAnyUser
                ,gpAlterAnyDatabase
                ,gpViewAnyDefinition
                ,gpAlter
                ,gpCreateAnyDatabase
                ,gpAuthenticateServer
                ,gpAlterAnySchema
                ,gpControl
                ,gpCreateStream
                ,gpAlterAnyRole
                ,gpAuthenticate
                ,gpRead
                ,gpBackupDatabase
                ,gpControlServer
                ,gpWrite
                ,gpViewAnyDatabase
                ,gpReferences
                ,gpCreateView
                ,gpConnect
                ,gpCreateTable
                ,gpAlterAnyLogin
                ,gpCreateEndpoint
                ,gpConnectEql
                ,gpTakeOwnership
                ,gpCreateRole
                ,gpCreateSource
                ,gpConnectAnyDatabase
                ,gpAlterAnyEndpoint
                ,gpCreateDatabase
                ,gpCreateSchema
            };
        }

        private List<SecurableClass> CreateSecurableClasses()
        {
            SecurableClass scDatabase = new SecurableClass { SecurableClassId = Guid.Parse("735EAA88-D1B1-4E1B-82B5-6CC2CD2F1DA3"), SecurableName = "database" };
            SecurableClass scDatabaserole = new SecurableClass { SecurableClassId = Guid.Parse("5E3520C3-3204-4763-9B44-F84C354369B8"), SecurableName = "databaserole" };
            SecurableClass scDatabaseuser = new SecurableClass { SecurableClassId = Guid.Parse("236A8A3F-C8AC-4047-B79E-4FECF5E691D4"), SecurableName = "databaseuser" };
            SecurableClass scEndpoint = new SecurableClass { SecurableClassId = Guid.Parse("ED9F18F4-BFDD-4DF6-BBD1-3CF33E294769"), SecurableName = "endpoint" };
            SecurableClass scLogin = new SecurableClass { SecurableClassId = Guid.Parse("F02FB277-7050-465C-BC9E-C90CDF6DE9DB"), SecurableName = "login" };
            SecurableClass scSchema = new SecurableClass { SecurableClassId = Guid.Parse("C6760E1C-03E3-4FCE-ADD2-9C112D8F222A"), SecurableName = "schema" };
            SecurableClass scServer = new SecurableClass { SecurableClassId = Guid.Parse("D0844276-DC31-4065-8E9D-8D787E0666DD"), SecurableName = "server" };
            SecurableClass scServerrole = new SecurableClass { SecurableClassId = Guid.Parse("F77D03D1-6506-419D-AA55-E2C1D54625CA"), SecurableName = "serverrole" };
            SecurableClass scSource = new SecurableClass { SecurableClassId = Guid.Parse("6FB2683D-544A-4DD7-9077-5A6E45FB9AE5"), SecurableName = "source" };
            SecurableClass scStream = new SecurableClass { SecurableClassId = Guid.Parse("ACCC3A64-56B6-4821-AB47-EC501F76ABD8"), SecurableName = "stream" };
            SecurableClass scView = new SecurableClass { SecurableClassId = Guid.Parse("E4F6BC86-4909-41A6-BA68-632DBEF8D6D7"), SecurableName = "view" };

            return new List<SecurableClass>
            {
                scDatabase
                ,scDatabaserole
                ,scDatabaseuser
                ,scEndpoint
                ,scLogin
                ,scSchema
                ,scServer
                ,scServerrole
                ,scSource
                ,scStream
                ,scView
            };
        }

        private List<SourceColumn> CreateInputSourceColumns()
        {
            SourceColumn inputSourceColumnProcessingCode = new SourceColumn { ColumnName = "ProcessingCode", ColumnType = "System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", ColumnId = Guid.Parse("86013C60-3E9A-497F-9AC0-010CF8FE84E4"), ColumnIndex = 3, ColumnLength = 4000 };
            SourceColumn inputSourceColumnLocalTransactionTime = new SourceColumn { ColumnName = "LocalTransactionTime", ColumnType = "System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", ColumnId = Guid.Parse("701478E9-7F7C-4267-ADAE-0DB19E3AF99E"), ColumnIndex = 7, ColumnLength = 4000 };
            SourceColumn inputSourceColumnRetrievalReferenceNumber = new SourceColumn { ColumnName = "RetrievalReferenceNumber", ColumnType = "System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", ColumnId = Guid.Parse("ADCB9471-6995-4F6D-BB51-12FCAF1FF962"), ColumnIndex = 16, ColumnLength = 4000 };
            SourceColumn inputSourceColumnTrack2Data = new SourceColumn { ColumnName = "Track2Data", ColumnType = "System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", ColumnId = Guid.Parse("220E993E-2440-4D90-A3F8-22F05352A50A"), ColumnIndex = 15, ColumnLength = 4000 };
            SourceColumn inputSourceColumnMessageType = new SourceColumn { ColumnName = "MessageType", ColumnType = "System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", ColumnId = Guid.Parse("2496719B-353F-43FE-81FE-25857652FF06"), ColumnIndex = 1, ColumnLength = 4000 };
            SourceColumn inputSourceColumnCardAcceptorIdentificationCode = new SourceColumn { ColumnName = "CardAcceptorIdentificationCode", ColumnType = "System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", ColumnId = Guid.Parse("0890AA73-7DA0-45DA-A020-3CB343E1E2B0"), ColumnIndex = 18, ColumnLength = 4000 };
            SourceColumn inputSourceColumnMerchantType = new SourceColumn { ColumnName = "MerchantType", ColumnType = "System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", ColumnId = Guid.Parse("58270EE5-8860-4445-84DF-4FE33C2B92C3"), ColumnIndex = 10, ColumnLength = 4000 };
            SourceColumn inputSourceColumnPointOfServiceEntryMode = new SourceColumn { ColumnName = "PointOfServiceEntryMode", ColumnType = "System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", ColumnId = Guid.Parse("7A5D827E-09F4-45A0-927F-5E5DCF20119C"), ColumnIndex = 12, ColumnLength = 4000 };
            SourceColumn inputSourceColumnTransactionCurrencyCode = new SourceColumn { ColumnName = "TransactionCurrencyCode", ColumnType = "System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", ColumnId = Guid.Parse("65BC026D-D942-49B4-9B50-61B727D6432F"), ColumnIndex = 20, ColumnLength = 4000 };
            SourceColumn inputSourceColumnPointOfServiceConditionCode = new SourceColumn { ColumnName = "PointOfServiceConditionCode", ColumnType = "System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", ColumnId = Guid.Parse("93F6CEC9-E1B8-49BC-8229-69C05EEAD6F6"), ColumnIndex = 13, ColumnLength = 4000 };
            SourceColumn inputSourceColumnLocalTransactionDate = new SourceColumn { ColumnName = "LocalTransactionDate", ColumnType = "System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", ColumnId = Guid.Parse("50D35C39-C3A7-4372-B197-6B74FDF7EC4E"), ColumnIndex = 8, ColumnLength = 4000 };
            SourceColumn inputSourceColumnCampo105 = new SourceColumn { ColumnName = "Campo105", ColumnType = "System.UInt32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", ColumnId = Guid.Parse("8275572F-C893-456A-9116-73AB0CFCF031"), ColumnIndex = 23, ColumnLength = null };
            SourceColumn inputSourceColumnTransactionAmount = new SourceColumn { ColumnName = "TransactionAmount", ColumnType = "System.Double, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", ColumnId = Guid.Parse("19DBBA51-97FF-4917-8E05-7D6F6A09B526"), ColumnIndex = 4, ColumnLength = null };
            SourceColumn inputSourceColumnSystemTraceAuditNumber = new SourceColumn { ColumnName = "SystemTraceAuditNumber", ColumnType = "System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", ColumnId = Guid.Parse("380194AB-5EDF-4339-8F58-8484FDACA88B"), ColumnIndex = 6, ColumnLength = 4000 };
            SourceColumn inputSourceColumnDateTimeTransmission = new SourceColumn { ColumnName = "DateTimeTransmission", ColumnType = "System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", ColumnId = Guid.Parse("E2DABDCE-FA62-4B99-92FD-98269DF95594"), ColumnIndex = 5, ColumnLength = 4000 };
            SourceColumn inputSourceColumnPrimaryAccountNumber = new SourceColumn { ColumnName = "PrimaryAccountNumber", ColumnType = "System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", ColumnId = Guid.Parse("1E15262D-262C-4576-9189-98AA4131DBA5"), ColumnIndex = 2, ColumnLength = 4000 };
            SourceColumn inputSourceColumnAccountIdentification1 = new SourceColumn { ColumnName = "AccountIdentification1", ColumnType = "System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", ColumnId = Guid.Parse("A7F43FF0-F129-44B4-8540-B163D5F92F8C"), ColumnIndex = 21, ColumnLength = 4000 };
            SourceColumn inputSourceColumnCampo104 = new SourceColumn { ColumnName = "Campo104", ColumnType = "System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", ColumnId = Guid.Parse("40907E6B-B3B8-4679-B4E0-C58DC2D6D501"), ColumnIndex = 22, ColumnLength = null };
            SourceColumn inputSourceColumnSetElementDate = new SourceColumn { ColumnName = "SetElementDate", ColumnType = "System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", ColumnId = Guid.Parse("BDE12DB7-D515-4330-9810-C87A2626434C"), ColumnIndex = 9, ColumnLength = 4000 };
            SourceColumn inputSourceColumnAcquiringInstitutionIdentificationCode = new SourceColumn { ColumnName = "AcquiringInstitutionIdentificationCode", ColumnType = "System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", ColumnId = Guid.Parse("76229424-8CA8-4C4C-A40F-E0B55DD109D1"), ColumnIndex = 14, ColumnLength = 4000 };
            SourceColumn inputSourceColumnCardAcceptorNameLocation = new SourceColumn { ColumnName = "CardAcceptorNameLocation", ColumnType = "System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", ColumnId = Guid.Parse("6E23722D-AC3A-4F5C-8665-F21EEFD08B41"), ColumnIndex = 19, ColumnLength = 4000 };
            SourceColumn inputSourceColumnCardAcceptorTerminalIdentification = new SourceColumn { ColumnName = "CardAcceptorTerminalIdentification", ColumnType = "System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", ColumnId = Guid.Parse("15829BE8-EF5C-41B1-9056-FC570A72EF56"), ColumnIndex = 17, ColumnLength = 4000 };
            SourceColumn inputSourceColumnAcquiringInstitutionCountryCode = new SourceColumn { ColumnName = "AcquiringInstitutionCountryCode", ColumnType = "System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", ColumnId = Guid.Parse("68B69FB0-CFF7-4747-B8E6-FEE4C7E9040E"), ColumnIndex = 11, ColumnLength = 4000 };

            return new List<SourceColumn>
            {
                inputSourceColumnProcessingCode
                ,inputSourceColumnLocalTransactionTime
                ,inputSourceColumnRetrievalReferenceNumber
                ,inputSourceColumnTrack2Data
                ,inputSourceColumnMessageType
                ,inputSourceColumnCardAcceptorIdentificationCode
                ,inputSourceColumnMerchantType
                ,inputSourceColumnPointOfServiceEntryMode
                ,inputSourceColumnTransactionCurrencyCode
                ,inputSourceColumnPointOfServiceConditionCode
                ,inputSourceColumnLocalTransactionDate
                ,inputSourceColumnCampo105
                ,inputSourceColumnTransactionAmount
                ,inputSourceColumnSystemTraceAuditNumber
                ,inputSourceColumnDateTimeTransmission
                ,inputSourceColumnPrimaryAccountNumber
                ,inputSourceColumnAccountIdentification1
                ,inputSourceColumnCampo104
                ,inputSourceColumnSetElementDate
                ,inputSourceColumnAcquiringInstitutionIdentificationCode
                ,inputSourceColumnCardAcceptorNameLocation
                ,inputSourceColumnCardAcceptorTerminalIdentification
                ,inputSourceColumnAcquiringInstitutionCountryCode
            };
        }

        private List<SourceColumn> CreateOutputSourceColumns()
        {
            SourceColumn outPutSourceColumnentero = new SourceColumn { ColumnName = "entero", ColumnType = "System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", ColumnId = Guid.Parse("9C3C369E-9E59-4C0A-8F5C-C402EEEDBD10"), ColumnIndex = 2, ColumnLength = null };
            SourceColumn outPutSourceColumnserverId = new SourceColumn { ColumnName = "serverId", ColumnType = "System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", ColumnId = Guid.Parse("1BF59FBD-F3F6-4555-8700-6E0548FB295A"), ColumnIndex = 1, ColumnLength = 4000 };

            return new List<SourceColumn>
            {
                outPutSourceColumnentero
                , outPutSourceColumnserverId
            };
        }

        private ServerRole CreateSysReaderServerRole()
        {
            return this.CreateServerRole(PersistentMockConstants.SYSREADER_SERVER_ROLE_NAME);
        }

        private ServerRole CreateSysAdminServerRole()
        {
            return this.CreateServerRole(PersistentMockConstants.SYSADMIN_SERVER_ROLE_NAME);
        }

        private Login CreateSaLogin()
        {
            return this.CreateLogin(PersistentMockConstants.SA_LOGIN_NAME, PersistentMockConstants.SA_LOGIN_PASSWORD);
        }

        private Database.Database CreateMasterDatabase()
        {
            return this.CreateDatabase(PersistentMockConstants.MASTER_DATABASE_NAME);
        }

        private Schema CreateDboSchema()
        {
            return this.CreateSchema(PersistentMockConstants.DBO_SCHEMA_NAME);
        }

        private Source CreateDefaultInputSource()
        {
            return this.CreateSource(PersistentMockConstants.INPUT_SOURCE_NAME);
        }

        private Source CreateDefaultOutputSource()
        {
            return this.CreateSource(PersistentMockConstants.OUTPUT_SOURCE_NAME);
        }
    }
}
