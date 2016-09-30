using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Integra.Space.Pipeline;
using Ninject;
using Integra.Space.Database;
using System.Data.Entity;

namespace Integra.Space.UnitTests
{
    [TestClass]
    public class CommandTests
    {
        private string loginName = "LoginAux";

        private FirstLevelPipelineContext ProcessCommand(string command, IKernel kernel)
        {
            CommandPipelineBuilder cpb = new CommandPipelineBuilder();
            Filter<FirstLevelPipelineContext, FirstLevelPipelineContext> pipeline = cpb.Build();

            FirstLevelPipelineExecutor cpe = new FirstLevelPipelineExecutor(pipeline);
            FirstLevelPipelineContext context = new FirstLevelPipelineContext(command, loginName, kernel);
            FirstLevelPipelineContext result = cpe.Execute(context);
            return result;
        }

        #region
        
        [TestMethod]
        public void GetMetadataTest()
        {
            string command = "from Streams as x where (string)ServerId == \"59e858fc-c84d-48a7-8a98-c0e7adede20a\" select ServerId as servId, max(1) as maxTest order by desc servId, maxTest";
            command = "from Streams as x select 1 as servId, 2 as maxTest";

            loginName = "AdminLogin";
            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    tran.Rollback();
                }
            }
        }

        #endregion

        #region take ownership

        [TestMethod]
        public void TakeOwnershipOnDbRole()
        {
            string entityName = "RoleForTest";
            string command = $"take ownership on role {entityName}";
            string databaseName = "Database1";
            loginName = "AdminLogin";
            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Login login = dbContext.Logins.Single(x => x.Server.ServerName == "Server1" && x.LoginName == loginName);
                    Database.Database database = dbContext.Databases.Single(x => x.ServerId == login.ServerId && x.DatabaseName == databaseName);
                    DatabaseRole role = dbContext.DatabaseRoles.Single(x => x.ServerId == login.ServerId && x.DatabaseId == database.DatabaseId && x.DbRoleName == entityName);
                    Assert.AreEqual<string>("AdminUser", role.DatabaseUser.DbUsrName);

                    Console.WriteLine();
                    tran.Rollback();
                }
            }
        }

        [TestMethod]
        public void TakeOwnershipOnDatabase()
        {
            string entityName = "Database1";
            string command = $"take ownership on database {entityName}";
            loginName = "AdminLogin";
            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Login login = dbContext.Logins.Single(x => x.Server.ServerName == "Server1" && x.LoginName == loginName);
                    Database.Database role = dbContext.Databases.Single(x => x.ServerId == login.ServerId && x.DatabaseName == entityName);
                    Assert.AreEqual<string>("AdminLogin", role.Login.LoginName);

                    Console.WriteLine();
                    tran.Rollback();
                }
            }
        }

        [TestMethod]
        public void TakeOwnershipOnEndpoint()
        {
            string entityName = "EndpointForTest";
            string command = $"take ownership on endpoint {entityName}";
            loginName = "AdminLogin";
            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Login login = dbContext.Logins.Single(x => x.Server.ServerName == "Server1" && x.LoginName == loginName);
                    Database.Endpoint role = dbContext.Endpoints.Single(x => x.ServerId == login.ServerId && x.EnpointName == entityName);
                    Assert.AreEqual<string>("AdminLogin", role.Login.LoginName);

                    Console.WriteLine();
                    tran.Rollback();
                }
            }
        }

        [TestMethod]
        public void TakeOwnershipOnSchema()
        {
            string entityName = "Schema1";
            string command = $"take ownership on schema {entityName}";
            string databaseName = "Database1";
            loginName = "AdminLogin";
            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Login login = dbContext.Logins.Single(x => x.Server.ServerName == "Server1" && x.LoginName == loginName);
                    Database.Database database = dbContext.Databases.Single(x => x.ServerId == login.ServerId && x.DatabaseName == databaseName);
                    Database.Schema role = dbContext.Schemas.Single(x => x.ServerId == login.ServerId && x.DatabaseId == database.DatabaseId && x.SchemaName == entityName);
                    Assert.AreEqual<string>("AdminUser", role.DatabaseUser.DbUsrName);

                    Console.WriteLine();
                    tran.Rollback();
                }
            }
        }

        [TestMethod]
        public void TakeOwnershipOnSource()
        {
            string entityName = "SourceInicial";
            string command = $"take ownership on source {entityName}";
            string databaseName = "Database1";
            string schemaName = "schema1";
            loginName = "AdminLogin";
            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Login login = dbContext.Logins.Single(x => x.Server.ServerName == "Server1" && x.LoginName == loginName);
                    Database.Database database = dbContext.Databases.Single(x => x.ServerId == login.ServerId && x.DatabaseName == databaseName);
                    Database.Schema schema = dbContext.Schemas.Single(x => x.ServerId == database.ServerId && x.DatabaseId == database.DatabaseId && x.SchemaName == schemaName);
                    Database.Source role = dbContext.Sources.Single(x => x.ServerId == login.ServerId && x.DatabaseId == database.DatabaseId && x.SchemaId == schema.SchemaId && x.SourceName == entityName);
                    Assert.AreEqual<string>("AdminUser", role.DatabaseUser.DbUsrName);

                    Console.WriteLine();
                    tran.Rollback();
                }
            }
        }

        [TestMethod]
        public void TakeOwnershipOnStream()
        {
            string entityName = "Stream123";
            string command = $"take ownership on stream {entityName}";
            string databaseName = "Database1";
            string schemaName = "schema1";
            loginName = "AdminLogin";
            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Login login = dbContext.Logins.Single(x => x.Server.ServerName == "Server1" && x.LoginName == loginName);
                    Database.Database database = dbContext.Databases.Single(x => x.ServerId == login.ServerId && x.DatabaseName == databaseName);
                    Database.Schema schema = dbContext.Schemas.Single(x => x.ServerId == database.ServerId && x.DatabaseId == database.DatabaseId && x.SchemaName == schemaName);
                    Database.Stream role = dbContext.Streams.Single(x => x.ServerId == login.ServerId && x.DatabaseId == database.DatabaseId && x.SchemaId == schema.SchemaId && x.StreamName == entityName);
                    Assert.AreEqual<string>("AdminUser", role.DatabaseUser.DbUsrName);

                    Console.WriteLine();
                    tran.Rollback();
                }
            }
        }

        #endregion take ownership

        [TestMethod]
        public void TestGrantPermissionOnDatabase()
        {
            loginName = "AdminLogin";
            string principalName = "AdminUser";
            string command = $"grant alter any user to user {principalName}";
            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    
                    Console.WriteLine();
                    tran.Rollback();
                }
            }
        }

        [TestMethod]
        public void TestCreateSchema()
        {
            loginName = "AdminLogin";
            string newSchemaName = "Schema123";
            string command = $"use Database1; create schema {newSchemaName}; create schema {newSchemaName + "XX"}; create schema {newSchemaName + "YY"}";
            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Server server = dbContext.Servers.Single(x => x.ServerName == "Server1");
                    Database.Database db = dbContext.Databases.Single(x => x.ServerId == server.ServerId && x.DatabaseName == "Database1");
                    Database.Schema schema1 = dbContext.Schemas.Single(x => x.ServerId == db.ServerId && x.DatabaseId == db.DatabaseId && x.SchemaName == newSchemaName);
                    Database.Schema schema2 = dbContext.Schemas.Single(x => x.ServerId == db.ServerId && x.DatabaseId == db.DatabaseId && x.SchemaName == newSchemaName + "XX");
                    Database.Schema schema3 = dbContext.Schemas.Single(x => x.ServerId == db.ServerId && x.DatabaseId == db.DatabaseId && x.SchemaName == newSchemaName + "YY");
                    Assert.IsNotNull(schema1);
                    Assert.IsNotNull(schema2);
                    Assert.IsNotNull(schema3);
                    Assert.AreEqual<string>(newSchemaName, schema1.SchemaName);
                    Assert.AreEqual<string>(newSchemaName + "XX", schema2.SchemaName);
                    Assert.AreEqual<string>(newSchemaName + "YY", schema3.SchemaName);

                    Console.WriteLine();
                    tran.Rollback();
                }
            }
        }
        
        [TestMethod]
        public void TestCreateStream()
        {
            loginName = "AdminLogin";
            string newStreamName = "Stream1234";
            string eql = "cross " +
                                  "JOIN SpaceObservable1 as t1 WHERE t1.@event.Message.#0.#0 == \"0100\" " +
                                  "WITH SpaceObservable1 as t2 WHERE t2.@event.Message.#0.#0 == \"0110\" " +
                                  "ON (string)t1.@event.Message.#1.#0 == (string)t2.@event.Message.#1.#0 and (string)t1.@event.Message.#1.#1 == (string)t2.@event.Message.#1.#1 " +
                                  "TIMEOUT '00:00:01.5' " +
                                  "WHERE isnull(t2.@event.SourceTimestamp, '01/01/2017') - isnull(t1.@event.SourceTimestamp, '01/01/2016') <= '00:00:01' " +
                                  "SELECT " +
                                          "t1.@event.Message.#1.#0 as c1, " +
                                          "t2.@event.Message.#1.#0 as c3 ";

            string command = $"create stream {newStreamName} {{\n{ eql }\n}}";
            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Server server = dbContext.Servers.Single(x => x.ServerName == "Server1");
                    Database.Database db = dbContext.Databases.Single(x => x.ServerId == server.ServerId && x.DatabaseName == "Database1");
                    Database.Schema schema = dbContext.Schemas.Single(x => x.ServerId == server.ServerId && x.DatabaseId == db.DatabaseId && x.SchemaName == "Schema1");
                    Database.Stream stream = dbContext.Streams.Single(x => x.ServerId == schema.ServerId && x.DatabaseId == schema.DatabaseId && x.SchemaId == schema.SchemaId && x.StreamName == newStreamName);
                    Assert.IsNotNull(stream);
                    Assert.AreEqual<string>(newStreamName, stream.StreamName);

                    Console.WriteLine();
                    tran.Rollback();
                }
            }
        }

        [TestMethod]
        public void TestCreateRole()
        {
            loginName = "AdminLogin";
            string newRoleName = "Role123";
            string command = "create role " + newRoleName;
            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Server server = dbContext.Servers.Single(x => x.ServerName == "Server1");
                    Database.Database db = dbContext.Databases.Single(x => x.DatabaseName == "Database1");
                    Database.DatabaseRole role = dbContext.DatabaseRoles.Single(x => x.ServerId == server.ServerId && x.DatabaseId == db.DatabaseId && x.DbRoleName == newRoleName);
                    Assert.IsNotNull(role);
                    Assert.AreEqual<string>(newRoleName, role.DbRoleName);

                    Console.WriteLine();
                    tran.Rollback();
                }
            }
        }

        [TestMethod]
        public void TestCreateUser()
        {
            string newUserName = "User123";
            string command = "create user " + newUserName + " with default_schema = Schema1";
            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Server server = dbContext.Servers.Single(x => x.ServerName == "Server1");
                    Database.Database db = dbContext.Databases.Single(x => x.DatabaseName == "Database1");
                    Database.DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.ServerId == server.ServerId && x.DatabaseId == db.DatabaseId && x.DbUsrName == newUserName);
                    Assert.IsNotNull(user);
                    Assert.AreEqual<string>(newUserName, user.DbUsrName);

                    Console.WriteLine();
                    tran.Rollback();
                }
            }
        }

        [TestMethod]
        public void TestCreateDatabase()
        {
            string newDatabaseName = "Database123";
            string command = "create database " + newDatabaseName;
            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Server server = dbContext.Servers.Single(x => x.ServerName == "Server1");
                    Database.Database db = dbContext.Databases.Single(x => x.DatabaseName == "Database1");
                    Database.Database database = dbContext.Databases.Single(x => x.ServerId == server.ServerId && x.DatabaseName == newDatabaseName);
                    Assert.IsNotNull(database);
                    Assert.AreEqual<string>(newDatabaseName, database.DatabaseName);

                    Console.WriteLine();
                    tran.Rollback();
                }
            }
        }

        [TestMethod]
        public void TestCreateLogin()
        {
            string newloginNAme = "Login123";
            string command = "create login " + newloginNAme + " with password = \"abc\", default_database = Database1";
            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Server server = dbContext.Servers.Single(x => x.ServerName == "Server1");
                    Database.Database db = dbContext.Databases.Single(x => x.DatabaseName == "Database1");
                    Database.Login login = dbContext.Logins.Single(x => x.ServerId == server.ServerId && x.LoginName== newloginNAme);
                    Assert.IsNotNull(login);
                    Assert.AreEqual<string>(newloginNAme, login.LoginName);

                    Console.WriteLine();
                    tran.Rollback();
                }
            }
        }

        [TestMethod]
        public void TestCreateSource()
        {
            string newSource = "SourceInicial_nueva";
            string command = "create source " + newSource;
            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Server server = dbContext.Servers.Single(x => x.ServerName == "Server1");
                    Database.Database db = dbContext.Databases.Single(x => x.DatabaseName == "Database1");
                    Database.Source source = dbContext.Sources.Single(x => x.ServerId == server.ServerId && x.DatabaseId == db.DatabaseId && x.SourceName == newSource);
                    Assert.IsNotNull(source);
                    Assert.AreEqual<string>(newSource, source.SourceName);

                    Console.WriteLine();
                    tran.Rollback();
                }
            }
        }
    }
}
