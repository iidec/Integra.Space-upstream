//-----------------------------------------------------------------------
// <copyright file="CommandTests.cs" company="ARITEC">
// Copyright (c) ARITEC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.UnitTests
{
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Threading.Tasks.Dataflow;
    using Compiler;
    using Database;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Ninject;
    using Ninject.Planning.Bindings;
    using Pipeline;

    /// <summary>
    /// A class that contains the test for space commands executed as an adminstrator login.
    /// </summary>
    [TestClass]
    public class CommandTests
    {
        /// <summary>
        /// Login to use in the tests.
        /// </summary>
        private string loginName = DatabaseConstants.NORMAL_LOGIN_3_NAME;

        /// <summary>
        /// Method to be called after each test.
        /// </summary>
        [TestInitialize]
        public void SetUp()
        {
            var dependency = typeof(System.Data.Entity.SqlServer.SqlProviderServices);
        }

        #region insert

        /// <summary>
        /// First insert an event to a source, then a stream receive it and finally the output result is validated.
        /// </summary>
        [TestMethod]
        public void Insert1()
        {
            #region commmand

            string command = $@"insert into {DatabaseConstants.INPUT_SOURCE_NAME}
                                (
                                MessageType,
                                PrimaryAccountNumber,
                                ProcessingCode,
                                TransactionAmount,
                                DateTimeTransmission,
                                SystemTraceAuditNumber,
                                LocalTransactionTime,
                                LocalTransactionDate,
                                SetElementDate,
                                MerchantType,
                                AcquiringInstitutionCountryCode,
                                PointOfServiceEntryMode,
                                PointOfServiceConditionCode,
                                AcquiringInstitutionIdentificationCode,
                                Track2Data,
                                RetrievalReferenceNumber,
                                CardAcceptorTerminalIdentification,
                                CardAcceptorIdentificationCode,
                                CardAcceptorNameLocation,
                                TransactionCurrencyCode,
                                AccountIdentification1,
                                Campo104,
                                Campo105
                                )
                                values
                                (
                                ""0100"",
                                ""9999941616073663"",
                                ""302000"",
                                1m,
                                ""0508152549"",
                                ""212868"",
                                ""152549"",
                                ""0508"",
                                ""0508"",
                                ""6011"",
                                ""320"",
                                ""051"",
                                ""02"",
                                ""491381"",
                                ""9999941616073663D18022011583036900000"",
                                ""412815212868"",
                                ""2906    "",
                                ""Shell El Rodeo "",
                                ""Shell El Rodeo1GUATEMALA    GT"",
                                ""320"",
                                ""00001613000000000001"",
                                -1,
                                1U
                                )";

            #endregion command

            this.loginName = DatabaseConstants.SA_LOGIN_NAME;
            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    #region create stream

                    string eql = $"use {DatabaseConstants.TEST_DATABASE_NAME}; from SourceParaPruebas where MessageType == \"0100\" select true as resultado into SourceParaPruebas";
                    ManagementSchedulerFactory dsf = new ManagementSchedulerFactory();
                    CodeGeneratorConfiguration context = this.GetCodeGeneratorConfig(dsf, kernel);
                    Language.CommandParser parser = new Language.CommandParser(eql, new TestRuleValidator());
                    Language.PlanNode executionPlan = ((Language.TemporalStreamNode)parser.Evaluate().Last()).ExecutionPlan;
                    CodeGenerator te = new CodeGenerator(context);
                    Assembly assembly = te.Compile(executionPlan);

                    Type[] types = assembly.GetTypes();
                    /*Type inputType = types.First(x => x.BaseType == typeof(InputBase));*/
                    Type queryInfo = types.First(x => x.GetInterface("IQueryInformation") == typeof(IQueryInformation));
                    IQueryInformation queryInfoObject = (IQueryInformation)Activator.CreateInstance(queryInfo);
                    Type queryType = queryInfoObject.GetQueryType();
                    object queryObject = Activator.CreateInstance(queryType);
                    MethodInfo result = queryObject.GetType().GetMethod("MainFunction");

                    IObservable<TestObject1> o = Observable.Create<TestObject1>(x =>
                    {
                        return BufferBlockForTest.BufferBlock1.AsObservable().Subscribe(y => x.OnNext((TestObject1)y));
                    });

                    IObservable<object> resultObservable = (IObservable<object>)result.Invoke(queryObject, new object[] { o /*, dsf.TestScheduler*/ });

                    #endregion create stream

                    resultObservable.Subscribe(x =>
                    {
                        bool val = bool.Parse(((Array)x.GetType().GetProperty("Result").GetValue(x)).GetValue(0).GetType().GetProperty("resultado").GetValue(((Array)x.GetType().GetProperty("Result").GetValue(x)).GetValue(0)).ToString());
                        System.Diagnostics.Debug.WriteLine(val);
                    });

                    try
                    {
                        FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                        tran.Rollback();
                    }
                    catch
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        #endregion insert

        #region metadata query

        /// <summary>
        /// Get the streams metadata using a query with from, where, apply window of, select, order by and into statements.
        /// </summary>
        [TestMethod]
        public void GetMetadataFromWhereSelectOrderByTest()
        {
            string command = $@"use {DatabaseConstants.TEST_DATABASE_NAME}; 
                                from sys.streams as x where (string)ServerId == ""59e858fc-c84d-48a7-8a98-c0e7adede20a"" 
                                apply window of '00:00:01' 
                                select ServerId as serverId, max(1) as entero 
                                order by desc serverId, entero 
                                into {DatabaseConstants.METADATA_OUTPUT_SOURCE_NAME}";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    kernel.Bind<ISourceTypeFactory>().ToConstructor(x => new SourceTypeFactory());
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    tran.Rollback();
                }
            }
        }

        /// <summary>
        /// Get the streams metadata using a query with from, select and into statements.
        /// </summary>
        [TestMethod]
        public void GetMetadataFromSelectTest()
        {
            string command = $@"use {DatabaseConstants.TEST_DATABASE_NAME};
                                from sys.streams as x select ServerId as serverId, 2 as entero into {DatabaseConstants.METADATA_OUTPUT_SOURCE_NAME}";

            this.loginName = DatabaseConstants.SA_LOGIN_NAME;
            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    kernel.Bind<ISourceTypeFactory>().ToConstructor(x => new SourceTypeFactory());
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    tran.Rollback();
                }
            }
        }

        #endregion metadata query

        #region take ownership

        /// <summary>
        /// Takes ownership of a database role.
        /// </summary>
        [TestMethod]
        public void TakeOwnershipOnDbRole()
        {
            string entityName = DatabaseConstants.ROLE_1_NAME;
            string command = $"take ownership on role {entityName}";
            string databaseName = DatabaseConstants.MASTER_DATABASE_NAME;
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;
            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Login login = dbContext.Logins.Single(x => x.Server.ServerName == DatabaseConstants.TEST_SERVER_NAME && x.LoginName == this.loginName);
                    Space.Database.Database database = dbContext.Databases.Single(x => x.ServerId == login.ServerId && x.DatabaseName == databaseName);
                    DatabaseRole role = dbContext.DatabaseRoles.Single(x => x.ServerId == login.ServerId && x.DatabaseId == database.DatabaseId && x.DbRoleName == entityName);
                    Assert.AreEqual<string>(DatabaseConstants.DBO_USER_NAME, role.DatabaseUser.DbUsrName);

                    Console.WriteLine();
                    tran.Rollback();
                }
            }
        }

        /// <summary>
        /// Takes ownership of a database.
        /// </summary>
        [TestMethod]
        public void TakeOwnershipOnDatabase()
        {
            string entityName = DatabaseConstants.TEST_DATABASE_NAME;
            string command = $"take ownership on database {entityName}";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;
            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Login login = dbContext.Logins.Single(x => x.Server.ServerName == DatabaseConstants.TEST_SERVER_NAME && x.LoginName == this.loginName);
                    Space.Database.Database database = dbContext.Databases.Single(x => x.ServerId == login.ServerId && x.DatabaseName == entityName);
                    Assert.AreEqual<string>(DatabaseConstants.SA_LOGIN_NAME, database.Login.LoginName);

                    Console.WriteLine();
                    tran.Rollback();
                }
            }
        }

        /// <summary>
        /// Takes ownership of an endpoint.
        /// </summary>
        [TestMethod]
        public void TakeOwnershipOnEndpoint()
        {
            string entityName = DatabaseConstants.TCP_ENDPOINT_NAME;
            string command = $"take ownership on endpoint {entityName}";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;
            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Login login = dbContext.Logins.Single(x => x.Server.ServerName == DatabaseConstants.TEST_SERVER_NAME && x.LoginName == this.loginName);
                    Space.Database.Endpoint endpoint = dbContext.Endpoints.Single(x => x.ServerId == login.ServerId && x.EnpointName == entityName);
                    Assert.AreEqual<string>(DatabaseConstants.SA_LOGIN_NAME, endpoint.Login.LoginName);

                    Console.WriteLine();
                    tran.Rollback();
                }
            }
        }

        /// <summary>
        /// Takes ownership of an schema.
        /// </summary>
        [TestMethod]
        public void TakeOwnershipOnSchema()
        {
            string entityName = DatabaseConstants.TEST_SCHEMA_NAME;
            string databaseName = DatabaseConstants.MASTER_DATABASE_NAME;
            string command = $"use {databaseName}; take ownership on schema {entityName}";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;
            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Login login = dbContext.Logins.Single(x => x.Server.ServerName == DatabaseConstants.TEST_SERVER_NAME && x.LoginName == this.loginName);
                    Space.Database.Database database = dbContext.Databases.Single(x => x.ServerId == login.ServerId && x.DatabaseName == databaseName);
                    Space.Database.Schema schema = dbContext.Schemas.Single(x => x.ServerId == login.ServerId && x.DatabaseId == database.DatabaseId && x.SchemaName == entityName);
                    Assert.AreEqual<string>(DatabaseConstants.DBO_USER_NAME, schema.DatabaseUser.DbUsrName);

                    Console.WriteLine();
                    tran.Rollback();
                }
            }
        }

        /// <summary>
        /// Takes ownership of a source.
        /// </summary>
        [TestMethod]
        public void TakeOwnershipOnSource()
        {
            string entityName = DatabaseConstants.INPUT_SOURCE_NAME;
            string databaseName = DatabaseConstants.MASTER_DATABASE_NAME;
            string command = $"take ownership on source {entityName}";
            string schemaName = DatabaseConstants.DBO_SCHEMA_NAME;
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;
            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Login login = dbContext.Logins.Single(x => x.Server.ServerName == DatabaseConstants.TEST_SERVER_NAME && x.LoginName == this.loginName);
                    Space.Database.Database database = dbContext.Databases.Single(x => x.ServerId == login.ServerId && x.DatabaseName == databaseName);
                    Space.Database.Schema schema = dbContext.Schemas.Single(x => x.ServerId == database.ServerId && x.DatabaseId == database.DatabaseId && x.SchemaName == schemaName);
                    Space.Database.Source source = dbContext.Sources.Single(x => x.ServerId == login.ServerId && x.DatabaseId == database.DatabaseId && x.SchemaId == schema.SchemaId && x.SourceName == entityName);
                    Assert.AreEqual<string>(DatabaseConstants.DBO_USER_NAME, source.DatabaseUser.DbUsrName);

                    Console.WriteLine();
                    tran.Rollback();
                }
            }
        }

        /// <summary>
        /// Takes ownership of a stream.
        /// </summary>
        [TestMethod]
        public void TakeOwnershipOnStream()
        {
            string entityName = DatabaseConstants.TEST_STREAM_NAME;
            string command = $"take ownership on stream {entityName}";
            string databaseName = DatabaseConstants.MASTER_DATABASE_NAME;
            string schemaName = DatabaseConstants.DBO_SCHEMA_NAME;
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;
            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Login login = dbContext.Logins.Single(x => x.Server.ServerName == DatabaseConstants.TEST_SERVER_NAME && x.LoginName == this.loginName);
                    Space.Database.Database database = dbContext.Databases.Single(x => x.ServerId == login.ServerId && x.DatabaseName == databaseName);
                    Space.Database.Schema schema = dbContext.Schemas.Single(x => x.ServerId == database.ServerId && x.DatabaseId == database.DatabaseId && x.SchemaName == schemaName);
                    Space.Database.Stream stream = dbContext.Streams.Single(x => x.ServerId == login.ServerId && x.DatabaseId == database.DatabaseId && x.SchemaId == schema.SchemaId && x.StreamName == entityName);
                    Assert.AreEqual<string>(DatabaseConstants.DBO_USER_NAME, stream.DatabaseUser.DbUsrName);

                    Console.WriteLine();
                    tran.Rollback();
                }
            }
        }

        #endregion take ownership

        #region add

        /// <summary>
        /// Add a user to a database role.
        /// </summary>
        [TestMethod]
        public void AddUserToRole()
        {
            string roleName = DatabaseConstants.ROLE_1_NAME;
            string userName = DatabaseConstants.NORMAL_USER_1_NAME;
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;
            string command = $"add {userName} to {roleName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        Space.Database.DatabaseUser dbUser = dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName).DatabaseUsers.Single(x => x.DbUsrName == userName);
                        Assert.AreEqual(userName, dbUser.DbUsrName);
                        tran.Rollback();
                    }
                    catch
                    {
                        tran.Rollback();
                        Assert.Fail($"Usuario '{userName}' no fue agregado al rol '{roleName}'");
                    }
                }
            }
        }

        /// <summary>
        /// Add a user to multiple roles.
        /// </summary>
        [TestMethod]
        public void AddUserToRoles()
        {
            string roleName1 = DatabaseConstants.ROLE_1_NAME;
            string roleName2 = DatabaseConstants.ROLE_2_NAME;
            string userName = DatabaseConstants.NORMAL_USER_1_NAME;
            string command = $"add {userName} to {roleName1}, {roleName2}";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        Space.Database.DatabaseUser dbUser1 = dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName1).DatabaseUsers.Single(x => x.DbUsrName == userName);
                        Assert.AreEqual(userName, dbUser1.DbUsrName);
                        Space.Database.DatabaseUser dbUser2 = dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName2).DatabaseUsers.Single(x => x.DbUsrName == userName);
                        Assert.AreEqual(userName, dbUser2.DbUsrName);
                        tran.Rollback();
                    }
                    catch
                    {
                        tran.Rollback();
                        Assert.Fail($"Usuario '{userName}' no fue agregado al rol '{roleName1}' o '{roleName2}'");
                    }
                }
            }
        }

        /// <summary>
        /// Add multiple users to a single database role.
        /// </summary>
        [TestMethod]
        public void AddUserListToRole()
        {
            string roleName = DatabaseConstants.ROLE_1_NAME;
            string userName1 = DatabaseConstants.NORMAL_USER_1_NAME;
            string userName2 = DatabaseConstants.NORMAL_USER_2_NAME;
            string command = $"add {userName1}, {userName2} to {roleName}";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        Space.Database.DatabaseUser dbUser1 = dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName).DatabaseUsers.Single(x => x.DbUsrName == userName1);
                        Assert.AreEqual(userName1, dbUser1.DbUsrName);
                        Space.Database.DatabaseUser dbUser2 = dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName).DatabaseUsers.Single(x => x.DbUsrName == userName2);
                        Assert.AreEqual(userName2, dbUser2.DbUsrName);
                        tran.Rollback();
                    }
                    catch
                    {
                        tran.Rollback();
                        Assert.Fail($"Usuario '{userName1}' o '{userName2}' no fue agregado al rol '{roleName}'");
                    }
                }
            }
        }

        /// <summary>
        /// Add multple users to multiple database roles.
        /// </summary>
        [TestMethod]
        public void AddUserListToRoles()
        {
            string roleName1 = DatabaseConstants.ROLE_1_NAME;
            string roleName2 = DatabaseConstants.ROLE_2_NAME;
            string userName1 = DatabaseConstants.NORMAL_USER_1_NAME;
            string userName2 = DatabaseConstants.NORMAL_USER_2_NAME;
            string command = $"add {userName1}, {userName2} to {roleName1}, {roleName2}";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        Space.Database.DatabaseUser dbUser1 = dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName1).DatabaseUsers.Single(x => x.DbUsrName == userName1);
                        Assert.AreEqual(userName1, dbUser1.DbUsrName);
                        Space.Database.DatabaseUser dbUser2 = dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName2).DatabaseUsers.Single(x => x.DbUsrName == userName1);
                        Assert.AreEqual(userName1, dbUser2.DbUsrName);

                        Space.Database.DatabaseUser dbUser3 = dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName1).DatabaseUsers.Single(x => x.DbUsrName == userName2);
                        Assert.AreEqual(userName2, dbUser3.DbUsrName);
                        Space.Database.DatabaseUser dbUser4 = dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName2).DatabaseUsers.Single(x => x.DbUsrName == userName2);
                        Assert.AreEqual(userName2, dbUser4.DbUsrName);

                        tran.Rollback();
                    }
                    catch
                    {
                        tran.Rollback();
                        Assert.Fail($"Usuario '{userName1}' o '{userName2}' no fue agregado al rol '{roleName1}' o '{roleName2}'");
                    }
                }
            }
        }

        #endregion add

        #region remove

        /// <summary>
        /// Removes a user from a role.
        /// </summary>
        [TestMethod]
        public void RemoveUserToRole()
        {
            string roleName = DatabaseConstants.ROLE_1_NAME;
            string userName = DatabaseConstants.NORMAL_USER_1_NAME;
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;
            string command = $"add {userName} to {roleName}";
            command += $"; remove {userName} to {roleName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        bool existe = dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName).DatabaseUsers.Any(x => x.DbUsrName == userName);
                        Assert.IsFalse(existe);
                        tran.Rollback();
                    }
                    catch
                    {
                        tran.Rollback();
                        Assert.Fail($"Usuario '{userName}' no fue agregado al rol '{roleName}'");
                    }
                }
            }
        }

        /// <summary>
        /// Removes a user from multiple roles.
        /// </summary>
        [TestMethod]
        public void RemoveUserToRoles()
        {
            string roleName1 = DatabaseConstants.ROLE_1_NAME;
            string roleName2 = DatabaseConstants.ROLE_2_NAME;
            string userName = DatabaseConstants.NORMAL_USER_1_NAME;
            string command = $"add {userName} to {roleName1}, {roleName2}";
            command += $"; remove {userName} to {roleName1}, {roleName2}";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        bool exists = dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName1).DatabaseUsers.Any(x => x.DbUsrName == userName);
                        exists = exists || dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName2).DatabaseUsers.Any(x => x.DbUsrName == userName);
                        Assert.IsFalse(exists);
                        tran.Rollback();
                    }
                    catch
                    {
                        tran.Rollback();
                        Assert.Fail($"Usuario '{userName}' no fue agregado al rol '{roleName1}' o '{roleName2}'");
                    }
                }
            }
        }

        /// <summary>
        /// Removes multiple users from a role.
        /// </summary>
        [TestMethod]
        public void RemoveUserListToRole()
        {
            string roleName = DatabaseConstants.ROLE_1_NAME;
            string userName1 = DatabaseConstants.NORMAL_USER_1_NAME;
            string userName2 = DatabaseConstants.NORMAL_USER_2_NAME;
            string command = $"add {userName1}, {userName2} to {roleName}";
            command += $"; remove {userName1}, {userName2} to {roleName}";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        bool exists = dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName).DatabaseUsers.Any(x => x.DbUsrName == userName1);
                        exists = exists || dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName).DatabaseUsers.Any(x => x.DbUsrName == userName2);
                        Assert.IsFalse(exists);
                        tran.Rollback();
                    }
                    catch
                    {
                        tran.Rollback();
                        Assert.Fail($"Usuario '{userName1}' o '{userName2}' no fue agregado al rol '{roleName}'");
                    }
                }
            }
        }

        /// <summary>
        /// Remove multiple users from multiple roles.
        /// </summary>
        [TestMethod]
        public void RemoveUserListToRoles()
        {
            string roleName1 = DatabaseConstants.ROLE_1_NAME;
            string roleName2 = DatabaseConstants.ROLE_2_NAME;
            string userName1 = DatabaseConstants.NORMAL_USER_1_NAME;
            string userName2 = DatabaseConstants.NORMAL_USER_2_NAME;
            string command = $"add {userName1}, {userName2} to {roleName1}, {roleName2}";
            command += $"; remove {userName1}, {userName2} to {roleName1}, {roleName2}";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        bool exists = dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName1).DatabaseUsers.Any(x => x.DbUsrName == userName1);
                        exists = exists || dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName2).DatabaseUsers.Any(x => x.DbUsrName == userName1);
                        exists = exists || dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName1).DatabaseUsers.Any(x => x.DbUsrName == userName2);
                        exists = exists || dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName2).DatabaseUsers.Any(x => x.DbUsrName == userName2);
                        Assert.IsFalse(exists);

                        tran.Rollback();
                    }
                    catch
                    {
                        tran.Rollback();
                        Assert.Fail($"Usuario '{userName1}' o '{userName2}' no fue agregado al rol '{roleName1}' o '{roleName2}'");
                    }
                }
            }
        }

        #endregion remove

        #region create

        #region create login

        /// <summary>
        /// Creates a new login without specifying options.
        /// </summary>
        [TestMethod]
        public void CreateLogin()
        {
            string loginName = "login1";
            string password = "pass1234";
            string command = $"create login {loginName} with password = \"{password}\"";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        Login login = dbContext.Logins.Single(x => x.LoginName == loginName);
                        Assert.AreEqual(loginName, login.LoginName);
                        Assert.AreEqual(password, login.LoginPassword);
                        tran.Rollback();
                    }
                    catch
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el login '{loginName}'.");
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new login with status on.
        /// </summary>
        [TestMethod]
        public void CreateLoginWithStatusOn()
        {
            string loginName = "login1";
            string password = "pass1234";
            string command = $"create login {loginName} with password = \"{password}\", status = on";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        Login login = dbContext.Logins.Single(x => x.LoginName == loginName);
                        Assert.AreEqual(loginName, login.LoginName);
                        Assert.AreEqual(password, login.LoginPassword);
                        Assert.IsTrue(login.IsActive);
                        tran.Rollback();
                    }
                    catch
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el login '{loginName}'.");
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new login with status off.
        /// </summary>
        [TestMethod]
        public void CreateLoginWithStatusOff()
        {
            string loginName = "login1";
            string password = "pass1234";
            string command = $"create login {loginName} with password = \"{password}\", status = off";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        Login login = dbContext.Logins.Single(x => x.LoginName == loginName);
                        Assert.AreEqual(loginName, login.LoginName);
                        Assert.AreEqual(password, login.LoginPassword);
                        Assert.IsFalse(login.IsActive);
                        tran.Rollback();
                    }
                    catch
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el login '{loginName}'.");
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new login with a default database.
        /// </summary>
        [TestMethod]
        public void CreateLoginWithDefaultDatabase()
        {
            string loginName = "login1";
            string password = "pass1234";
            string databaseName = DatabaseConstants.MASTER_DATABASE_NAME;
            string command = $"create login {loginName} with password = \"{password}\", default_database = {databaseName}";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        Login login = dbContext.Logins.Single(x => x.LoginName == loginName);
                        Assert.AreEqual(loginName, login.LoginName);
                        Assert.AreEqual(password, login.LoginPassword);
                        Assert.AreEqual(databaseName, login.Database.DatabaseName);
                        tran.Rollback();
                    }
                    catch
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el login '{loginName}'.");
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new login with a default database and status on.
        /// </summary>
        [TestMethod]
        public void CreateLoginWithDefaultDatabaseAndStatusOn()
        {
            string loginName = "login1";
            string password = "pass1234";
            string databaseName = DatabaseConstants.MASTER_DATABASE_NAME;
            string command = $"create login {loginName} with password = \"{password}\", default_database = {databaseName}, status = on";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        Login login = dbContext.Logins.Single(x => x.LoginName == loginName);
                        Assert.AreEqual(loginName, login.LoginName);
                        Assert.AreEqual(password, login.LoginPassword);
                        Assert.AreEqual(databaseName, login.Database.DatabaseName);
                        Assert.IsTrue(login.IsActive);
                        tran.Rollback();
                    }
                    catch
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el login '{loginName}'.");
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new login with a default database and status off.
        /// </summary>
        [TestMethod]
        public void CreateLoginWithDefaultDatabaseAndStatusOff()
        {
            string loginName = "login1";
            string password = "pass1234";
            string databaseName = DatabaseConstants.MASTER_DATABASE_NAME;
            string command = $"create login {loginName} with password = \"{password}\", default_database = {databaseName}, status = off";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        Login login = dbContext.Logins.Single(x => x.LoginName == loginName);
                        Assert.AreEqual(loginName, login.LoginName);
                        Assert.AreEqual(password, login.LoginPassword);
                        Assert.AreEqual(databaseName, login.Database.DatabaseName);
                        Assert.IsFalse(login.IsActive);
                        tran.Rollback();
                    }
                    catch
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el login '{loginName}'.");
                    }
                }
            }
        }

        #endregion create login

        #region create database

        /// <summary>
        /// Creates a new database without specifying options.
        /// </summary>
        [TestMethod]
        public void CreateDatabase()
        {
            string databaseName = "newDatabase";
            string command = $"create database {databaseName}";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        Space.Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == databaseName);
                        Assert.AreEqual(databaseName, database.DatabaseName);
                        tran.Rollback();
                    }
                    catch
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear la base de datos '{databaseName}'.");
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new database with status on.
        /// </summary>
        [TestMethod]
        public void CreateDatabaseWithStatusOn()
        {
            string databaseName = "newDatabase";
            string command = $"create database {databaseName} with status = on";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        Space.Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == databaseName);
                        Assert.AreEqual(databaseName, database.DatabaseName);
                        Assert.IsTrue(database.IsActive);
                        tran.Rollback();
                    }
                    catch
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear la base de datos '{databaseName}'.");
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new database with status off.
        /// </summary>
        [TestMethod]
        public void CreateDatabaseWithStatusOff()
        {
            string databaseName = "newDatabase";
            string command = $"create database {databaseName} with status = off";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        Space.Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == databaseName);
                        Assert.AreEqual(databaseName, database.DatabaseName);
                        Assert.IsFalse(database.IsActive);
                        tran.Rollback();
                    }
                    catch
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear la base de datos '{databaseName}'.");
                    }
                }
            }
        }

        #endregion create database

        #region create user

        /// <summary>
        /// Creates a new user without specifying options.
        /// </summary>
        [TestMethod]
        public void CreateUserWithLogin()
        {
            string userName = DatabaseConstants.NORMAL_LOGIN_2_NAME;
            string loginUserName = DatabaseConstants.NORMAL_LOGIN_2_NAME;
            string command = $"use {DatabaseConstants.TEST_DATABASE_NAME}; create user {userName} with login = {loginUserName}";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.Database.Server.ServerName == DatabaseConstants.TEST_SERVER_NAME && x.Database.DatabaseName == DatabaseConstants.TEST_DATABASE_NAME && x.DbUsrName == userName);
                        Assert.AreEqual(userName, user.DbUsrName);
                        Assert.AreEqual(loginUserName, user.Login.LoginName);
                        Assert.IsTrue(user.IsActive);
                        tran.Rollback();
                    }
                    catch
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear la base de datos '{userName}'.");
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new user with default schema.
        /// </summary>
        [TestMethod]
        public void CreateUserWithLoginDefaultSchema()
        {
            string userName = DatabaseConstants.NORMAL_LOGIN_2_NAME;
            string loginUserName = DatabaseConstants.NORMAL_LOGIN_2_NAME;
            string schemaName = DatabaseConstants.DBO_SCHEMA_NAME;
            string command = $"use {DatabaseConstants.TEST_DATABASE_NAME}; create user {userName} with login = {loginUserName}, default_schema = {schemaName}";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.Database.Server.ServerName == DatabaseConstants.TEST_SERVER_NAME && x.Database.DatabaseName == DatabaseConstants.TEST_DATABASE_NAME && x.DbUsrName == userName);
                        Assert.AreEqual(userName, user.DbUsrName);
                        Assert.AreEqual(loginUserName, user.Login.LoginName);
                        Assert.AreEqual(schemaName, user.DefaultSchema.SchemaName);
                        Assert.IsTrue(user.IsActive);
                        tran.Rollback();
                    }
                    catch
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear la base de datos '{userName}'.");
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new user with status on.
        /// </summary>
        [TestMethod]
        public void CreateUserWithStatusOn()
        {
            string userName = DatabaseConstants.NORMAL_USER_2_NAME;
            string loginUserName = DatabaseConstants.NORMAL_LOGIN_2_NAME;
            string command = $"use {DatabaseConstants.TEST_DATABASE_NAME}; create user {userName} with login = {loginUserName}, status = on";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.Database.Server.ServerName == DatabaseConstants.TEST_SERVER_NAME && x.Database.DatabaseName == DatabaseConstants.TEST_DATABASE_NAME && x.DbUsrName == userName);
                        Assert.AreEqual(userName, user.DbUsrName);
                        Assert.AreEqual(loginUserName, user.Login.LoginName);
                        Assert.IsTrue(user.IsActive);
                        tran.Rollback();
                    }
                    catch
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear la base de datos '{userName}'.");
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new user with status off.
        /// </summary>
        [TestMethod]
        public void CreateUserWithStatusOff()
        {
            string userName = DatabaseConstants.NORMAL_LOGIN_2_NAME;
            string loginUserName = DatabaseConstants.NORMAL_LOGIN_2_NAME;
            string command = $"use {DatabaseConstants.TEST_DATABASE_NAME}; create user {userName} with login = {loginUserName}, status = off";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.Database.Server.ServerName == DatabaseConstants.TEST_SERVER_NAME && x.Database.DatabaseName == DatabaseConstants.TEST_DATABASE_NAME && x.DbUsrName == userName);
                        Assert.AreEqual(userName, user.DbUsrName);
                        Assert.AreEqual(loginUserName, user.Login.LoginName);
                        Assert.IsFalse(user.IsActive);
                        tran.Rollback();
                    }
                    catch
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear la base de datos '{userName}'.");
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new user with default schema and status off.
        /// </summary>
        [TestMethod]
        public void CreateUserWithDefaultSchemaLoginStatusOff()
        {
            string userName = DatabaseConstants.NORMAL_LOGIN_2_NAME;
            string loginUserName = DatabaseConstants.NORMAL_LOGIN_2_NAME;
            string schemaName = DatabaseConstants.DBO_SCHEMA_NAME;
            string command = $"use {DatabaseConstants.TEST_DATABASE_NAME}; create user {userName} with login = {loginUserName}, default_schema = {schemaName}, status = off";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.Database.Server.ServerName == DatabaseConstants.TEST_SERVER_NAME && x.Database.DatabaseName == DatabaseConstants.TEST_DATABASE_NAME && x.DbUsrName == userName);
                        Assert.AreEqual(userName, user.DbUsrName);
                        Assert.AreEqual(loginUserName, user.Login.LoginName);
                        Assert.AreEqual(schemaName, user.DefaultSchema.SchemaName);
                        Assert.IsFalse(user.IsActive);
                        tran.Rollback();
                    }
                    catch
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear la base de datos '{userName}'.");
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new user with default schema and status on.
        /// </summary>
        [TestMethod]
        public void CreateUserWithDefaultSchemaLoginStatusOn()
        {
            string userName = DatabaseConstants.NORMAL_LOGIN_2_NAME;
            string loginUserName = DatabaseConstants.NORMAL_LOGIN_2_NAME;
            string schemaName = DatabaseConstants.DBO_SCHEMA_NAME;
            string command = $"use {DatabaseConstants.TEST_DATABASE_NAME}; create user {userName} with login = {loginUserName}, default_schema = {schemaName}, status = on";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.Database.Server.ServerName == DatabaseConstants.TEST_SERVER_NAME && x.Database.DatabaseName == DatabaseConstants.TEST_DATABASE_NAME && x.DbUsrName == userName);
                        Assert.AreEqual(userName, user.DbUsrName);
                        Assert.AreEqual(loginUserName, user.Login.LoginName);
                        Assert.AreEqual(schemaName, user.DefaultSchema.SchemaName);
                        Assert.IsTrue(user.IsActive);
                        tran.Rollback();
                    }
                    catch
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear la base de datos '{userName}'.");
                    }
                }
            }
        }

        #endregion create user

        #region create role

        /// <summary>
        /// Creates a new database role without specifying options.
        /// </summary>
        [TestMethod]
        public void CreateRole()
        {
            string roleName = DatabaseConstants.INEXISTENT_ROLE_NAME_1;
            string command = $"use {DatabaseConstants.TEST_DATABASE_NAME}; create role {roleName}";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        DatabaseRole role = dbContext.DatabaseRoles.Single(x => x.Database.Server.ServerName == DatabaseConstants.TEST_SERVER_NAME && x.Database.DatabaseName == DatabaseConstants.TEST_DATABASE_NAME && x.DbRoleName == roleName);
                        Assert.AreEqual(roleName, role.DbRoleName);
                        Assert.IsTrue(role.IsActive);
                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el rol de base de datos '{roleName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new role with status on.
        /// </summary>
        [TestMethod]
        public void CreateRoleWithStatusOn()
        {
            string roleName = DatabaseConstants.INEXISTENT_ROLE_NAME_1;
            string command = $"use {DatabaseConstants.TEST_DATABASE_NAME}; create role {roleName} with status = on";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        DatabaseRole role = dbContext.DatabaseRoles.Single(x => x.Database.Server.ServerName == DatabaseConstants.TEST_SERVER_NAME && x.Database.DatabaseName == DatabaseConstants.TEST_DATABASE_NAME && x.DbRoleName == roleName);
                        Assert.AreEqual(roleName, role.DbRoleName);
                        Assert.IsTrue(role.IsActive);
                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el rol de base de datos '{roleName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new role with status off.
        /// </summary>
        [TestMethod]
        public void CreateRoleWithStatusOff()
        {
            string roleName = DatabaseConstants.INEXISTENT_ROLE_NAME_1;
            string command = $"use {DatabaseConstants.TEST_DATABASE_NAME}; create role {roleName} with status = off";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        DatabaseRole role = dbContext.DatabaseRoles.Single(x => x.Database.Server.ServerName == DatabaseConstants.TEST_SERVER_NAME && x.Database.DatabaseName == DatabaseConstants.TEST_DATABASE_NAME && x.DbRoleName == roleName);
                        Assert.AreEqual(roleName, role.DbRoleName);
                        Assert.IsFalse(role.IsActive);
                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el rol de base de datos '{roleName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new role with a user.
        /// </summary>
        [TestMethod]
        public void CreateRoleAddUser()
        {
            string roleName = DatabaseConstants.INEXISTENT_ROLE_NAME_1;
            string userName = DatabaseConstants.NORMAL_USER_2_NAME;
            string command = $"create role {roleName} with add = {userName}";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        DatabaseRole role = dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName);
                        Assert.AreEqual(roleName, role.DbRoleName);
                        Assert.IsTrue(role.IsActive);

                        DatabaseUser user = role.DatabaseUsers.Single(x => x.ServerId == role.ServerId && x.DatabaseId == role.DatabaseId && x.DbUsrName == userName);
                        Assert.AreEqual(userName, user.DbUsrName);
                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el rol de base de datos '{roleName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new role with users.
        /// </summary>
        [TestMethod]
        public void CreateRoleAddUsers()
        {
            string roleName = DatabaseConstants.INEXISTENT_ROLE_NAME_1;
            string userName1 = DatabaseConstants.NORMAL_USER_2_NAME;
            string userName2 = DatabaseConstants.NORMAL_USER_1_NAME;
            string userName3 = DatabaseConstants.NORMAL_USER_3_NAME;
            string command = $"create role {roleName} with add = {userName1} {userName2} {userName3}";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        DatabaseRole role = dbContext.DatabaseRoles.Single(x => x.Database.Server.ServerName == DatabaseConstants.TEST_SERVER_NAME && x.Database.DatabaseName == DatabaseConstants.MASTER_DATABASE_NAME && x.DbRoleName == roleName);
                        Assert.AreEqual(roleName, role.DbRoleName);
                        Assert.IsTrue(role.IsActive);

                        DatabaseUser user = role.DatabaseUsers.Single(x => x.ServerId == role.ServerId && x.DatabaseId == role.DatabaseId && x.DbUsrName == userName1);
                        Assert.AreEqual(userName1, user.DbUsrName);
                        user = role.DatabaseUsers.Single(x => x.ServerId == role.ServerId && x.DatabaseId == role.DatabaseId && x.DbUsrName == userName2);
                        Assert.AreEqual(userName2, user.DbUsrName);
                        user = role.DatabaseUsers.Single(x => x.ServerId == role.ServerId && x.DatabaseId == role.DatabaseId && x.DbUsrName == userName3);
                        Assert.AreEqual(userName3, user.DbUsrName);
                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el rol de base de datos '{roleName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        #endregion create role

        #region create schema

        /// <summary>
        /// Creates a new schema without specifying opitons.
        /// </summary>
        [TestMethod]
        public void CreateSchema()
        {
            string schemaName = "newSchema";
            string command = $"create schema {schemaName}";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        Schema schema = dbContext.Schemas.Single(x => x.SchemaName == schemaName);
                        Assert.AreEqual(schemaName, schema.SchemaName);
                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el rol de base de datos '{schemaName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        #endregion create schema

        #region create source

        /// <summary>
        /// Creates a new source without specifying option.
        /// </summary>
        [TestMethod]
        public void CreateSource()
        {
            string sourceName = "newSource";
            string command = $"create source {sourceName} (column1 int, column2 double, column3 string(4000))";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();

            SpaceAssemblyBuilder sasmBuilder = new SpaceAssemblyBuilder("Integra.Space.Sources");
            AssemblyBuilder asmBuilder = sasmBuilder.CreateAssemblyBuilder();
            SpaceModuleBuilder modBuilder = new SpaceModuleBuilder(asmBuilder);
            modBuilder.CreateModuleBuilder();
            kernel.Bind<AssemblyBuilder>().ToConstant(asmBuilder);

            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        Source source = dbContext.Sources.Single(x => x.SourceName == sourceName);
                        Assert.AreEqual(sourceName, source.SourceName);
                        Assert.IsTrue(dbContext.SourceColumns.Any(x => x.ColumnName == "column1"));
                        Assert.IsTrue(dbContext.SourceColumns.Any(x => x.ColumnName == "column2"));
                        Assert.IsTrue(dbContext.SourceColumns.Any(x => x.ColumnName == "column3"));
                        Assert.IsTrue(source.IsActive);
                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el rol de base de datos '{sourceName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new source with status on.
        /// </summary>
        [TestMethod]
        public void CreateSourceWithStatusOn()
        {
            string sourceName = "newSource";
            string command = $"create source {sourceName} (column1 int, column2 double, column3 string(4000)) with status = on";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    SpaceAssemblyBuilder sasmBuilder = new SpaceAssemblyBuilder("Test");
                    AssemblyBuilder asmBuilder = sasmBuilder.CreateAssemblyBuilder();
                    SpaceModuleBuilder smodBuilder = new SpaceModuleBuilder(asmBuilder);
                    smodBuilder.CreateModuleBuilder();
                    kernel.Bind<AssemblyBuilder>().ToConstant(asmBuilder);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        Source source = dbContext.Sources.Single(x => x.SourceName == sourceName);
                        Assert.AreEqual(sourceName, source.SourceName);
                        Assert.IsTrue(source.IsActive);
                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el rol de base de datos '{sourceName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new source with status off.
        /// </summary>
        [TestMethod]
        public void CreateSourceWithStatusOff()
        {
            string sourceName = "newSource";
            string command = $"create source {sourceName} (column1 int, column2 double, column3 string(4000)) with status = off";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    SpaceAssemblyBuilder sasmBuilder = new SpaceAssemblyBuilder("Test");
                    AssemblyBuilder asmBuilder = sasmBuilder.CreateAssemblyBuilder();
                    SpaceModuleBuilder smodBuilder = new SpaceModuleBuilder(asmBuilder);
                    smodBuilder.CreateModuleBuilder();
                    kernel.Bind<AssemblyBuilder>().ToConstant(asmBuilder);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        Source source = dbContext.Sources.Single(x => x.SourceName == sourceName);
                        Assert.AreEqual(sourceName, source.SourceName);
                        Assert.IsFalse(source.IsActive);
                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el rol de base de datos '{sourceName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new source with cache size.
        /// </summary>
        [TestMethod]
        public void CreateSourceWithCacheSize()
        {
            string sourceName = "newSource";
            string command = $"create source {sourceName} (column1 int, column2 double, column3 string(4000)) with cache_size = 200";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    SpaceAssemblyBuilder sasmBuilder = new SpaceAssemblyBuilder("Test");
                    AssemblyBuilder asmBuilder = sasmBuilder.CreateAssemblyBuilder();
                    SpaceModuleBuilder smodBuilder = new SpaceModuleBuilder(asmBuilder);
                    smodBuilder.CreateModuleBuilder();
                    kernel.Bind<AssemblyBuilder>().ToConstant(asmBuilder);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        Source source = dbContext.Sources.Single(x => x.SourceName == sourceName);
                        Assert.IsTrue(dbContext.SourceColumns.Any(x => x.ColumnName == "column1"));
                        Assert.IsTrue(dbContext.SourceColumns.Any(x => x.ColumnName == "column2"));
                        Assert.IsTrue(dbContext.SourceColumns.Any(x => x.ColumnName == "column3"));
                        Assert.AreEqual(sourceName, source.SourceName);
                        Assert.AreEqual<uint>(200, (uint)source.CacheSize);
                        Assert.AreEqual<uint>(60, (uint)source.CacheDurability);
                        Assert.IsTrue(source.IsActive);
                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el rol de base de datos '{sourceName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new source with cache durability.
        /// </summary>
        [TestMethod]
        public void CreateSourceWithCacheDurability()
        {
            string sourceName = "newSource";
            string command = $"create source {sourceName} (column1 int, column2 double, column3 string(4000)) with cache_durability = 70";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    SpaceAssemblyBuilder sasmBuilder = new SpaceAssemblyBuilder("Test");
                    AssemblyBuilder asmBuilder = sasmBuilder.CreateAssemblyBuilder();
                    SpaceModuleBuilder smodBuilder = new SpaceModuleBuilder(asmBuilder);
                    smodBuilder.CreateModuleBuilder();
                    kernel.Bind<AssemblyBuilder>().ToConstant(asmBuilder);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        Source source = dbContext.Sources.Single(x => x.SourceName == sourceName);
                        Assert.IsTrue(dbContext.SourceColumns.Any(x => x.ColumnName == "column1"));
                        Assert.IsTrue(dbContext.SourceColumns.Any(x => x.ColumnName == "column2"));
                        Assert.IsTrue(dbContext.SourceColumns.Any(x => x.ColumnName == "column3"));
                        Assert.AreEqual(sourceName, source.SourceName);
                        Assert.AreEqual<uint>(100, (uint)source.CacheSize);
                        Assert.AreEqual<uint>(70, (uint)source.CacheDurability);
                        Assert.IsTrue(source.IsActive);
                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el rol de base de datos '{sourceName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new source with cache durability and cache size.
        /// </summary>
        [TestMethod]
        public void CreateSourceWithCacheDurabilityCacheSize()
        {
            string sourceName = "newSource";
            string command = $"create source {sourceName} (column1 int, column2 double, column3 string(4000)) with cache_durability = 70, cache_size = 200";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    SpaceAssemblyBuilder sasmBuilder = new SpaceAssemblyBuilder("Test");
                    AssemblyBuilder asmBuilder = sasmBuilder.CreateAssemblyBuilder();
                    SpaceModuleBuilder smodBuilder = new SpaceModuleBuilder(asmBuilder);
                    smodBuilder.CreateModuleBuilder();
                    kernel.Bind<AssemblyBuilder>().ToConstant(asmBuilder);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        Source source = dbContext.Sources.Single(x => x.SourceName == sourceName);
                        Assert.IsTrue(dbContext.SourceColumns.Any(x => x.ColumnName == "column1"));
                        Assert.IsTrue(dbContext.SourceColumns.Any(x => x.ColumnName == "column2"));
                        Assert.IsTrue(dbContext.SourceColumns.Any(x => x.ColumnName == "column3"));
                        Assert.AreEqual(sourceName, source.SourceName);
                        Assert.AreEqual<uint>(200, (uint)source.CacheSize);
                        Assert.AreEqual<uint>(70, (uint)source.CacheDurability);
                        Assert.IsTrue(source.IsActive);
                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el rol de base de datos '{sourceName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        #endregion create source

        #region create stream

        /// <summary>
        /// Creates a new stream without specifying options.
        /// </summary>
        [TestMethod]
        public void CreateStream()
        {
            string streamName = "newStream";
            string sourceName = "newSource";
            string eql = $@"cross " +
                                   $@"JOIN {DatabaseConstants.INPUT_SOURCE_NAME} as t1 WHERE t1.PrimaryAccountNumber == ""9999941616073663_1"" " +
                                   $@"WITH {DatabaseConstants.INPUT_SOURCE_NAME} as t2 WHERE t2.PrimaryAccountNumber == ""9999941616073663_2"" " +
                                   $@"ON t1.AcquiringInstitutionIdentificationCode == t2.AcquiringInstitutionIdentificationCode " +
                                   $@"TIMEOUT '00:00:02' " +
                                   /*WHERE  t1.@event.Message.#1.#43 == ""Shell El RodeoGUATEMALA    GT""*/
                                   $@"SELECT (string)t1.PrimaryAccountNumber as c1, t2.PrimaryAccountNumber as c2, 1 as numeroXXX into {sourceName} ";

            string command = $"create source {sourceName} (c1 string(4000), c2 string(4000), numeroXXX int); create stream {streamName} {{ {eql} }}";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    SpaceAssemblyBuilder sasmBuilder = new SpaceAssemblyBuilder("Test");
                    AssemblyBuilder asmBuilder = sasmBuilder.CreateAssemblyBuilder();
                    SpaceModuleBuilder smodBuilder = new SpaceModuleBuilder(asmBuilder);
                    smodBuilder.CreateModuleBuilder();
                    kernel.Bind<AssemblyBuilder>().ToConstant(asmBuilder);
                    kernel.Bind<ISourceTypeFactory>().ToConstructor(x => new SourceTypeFactory());
                    kernel.Bind<ISource>().ToConstructor(x => new ConcreteSource());

                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        Stream stream = dbContext.Streams.Single(x => x.Schema.Database.Server.ServerName == DatabaseConstants.TEST_SERVER_NAME && x.Schema.Database.DatabaseName == DatabaseConstants.MASTER_DATABASE_NAME && x.StreamName == streamName);
                        Assert.AreEqual(streamName, stream.StreamName);
                        Assert.IsTrue(stream.IsActive);
                        Assert.AreEqual(eql.Replace('\n', '\0').Trim(), stream.Query);
                        StreamColumn[] projectionColumns = dbContext.StreamColumns.Where(x => x.ServerId == stream.ServerId
                                            && x.DatabaseId == stream.DatabaseId
                                            && x.SchemaId == stream.SchemaId
                                            && x.StreamId == stream.StreamId).ToArray();

                        Assert.IsTrue(projectionColumns.Any(x => x.ColumnName == "c1" && x.ColumnType == typeof(string).AssemblyQualifiedName));
                        Assert.IsTrue(projectionColumns.Any(x => x.ColumnName == "c2" && x.ColumnType == typeof(string).AssemblyQualifiedName));
                        Assert.IsTrue(projectionColumns.Any(x => x.ColumnName == "numeroXXX" && x.ColumnType == typeof(int).AssemblyQualifiedName));

                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el flujo de eventos '{streamName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Creates a stream referencing a incompatible source.
        /// </summary>
        [TestMethod]
        public void CreateStreamSourceIncompatible()
        {
            string streamName = "newStream";
            string sourceName = "newSource";
            string eql = "cross " +
                                   "JOIN SourceInicial as t1 WHERE t1.PrimaryAccountNumber == \"9999941616073663_1\" " +
                                   "WITH SourceInicial as t2 WHERE t2.PrimaryAccountNumber == \"9999941616073663_2\" " +
                                   "ON t1.AcquiringInstitutionIdentificationCode == t2.AcquiringInstitutionIdentificationCode " +
                                   "TIMEOUT '00:00:02' " +
                                   $"SELECT t1.PrimaryAccountNumber as c1, t2.PrimaryAccountNumber as c2, 1 as numeroXXX into {sourceName} ";

            string command = $"create source {sourceName} (c1 string(4000), c2 string(4000)); create stream {streamName} {{ {eql} }}";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    try
                    {
                        FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                        tran.Rollback();
                        Assert.Fail("Un stream con proyección incompatible con la funente especificada en el into fue creado.");
                    }
                    catch (Exception)
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Creates a stream with status on.
        /// </summary>
        [TestMethod]
        public void CreateStreamWithStatusOn()
        {
            string streamName = "newStream";
            string sourceName = "newSource";
            string eql = "cross " +
                                   $@"JOIN {DatabaseConstants.INPUT_SOURCE_NAME} as t1 WHERE t1.PrimaryAccountNumber == ""9999941616073663_1"" " +
                                   $@"WITH {DatabaseConstants.INPUT_SOURCE_NAME} as t2 WHERE t2.PrimaryAccountNumber == ""9999941616073663_2"" " +
                                   "ON t1.AcquiringInstitutionIdentificationCode == t2.AcquiringInstitutionIdentificationCode " +
                                   "TIMEOUT '00:00:02' " +
                                   $"SELECT (string)t1.PrimaryAccountNumber as c1, t2.PrimaryAccountNumber as c2, 1 as numeroXXX into {sourceName} ";

            string command = $"create source {sourceName} (c1 string(4000), c2 string(4000), numeroXXX int); create stream {streamName} {{ {eql} }} with status = on";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    SpaceAssemblyBuilder sasmBuilder = new SpaceAssemblyBuilder("Test");
                    AssemblyBuilder asmBuilder = sasmBuilder.CreateAssemblyBuilder();
                    SpaceModuleBuilder smodBuilder = new SpaceModuleBuilder(asmBuilder);
                    smodBuilder.CreateModuleBuilder();
                    kernel.Bind<AssemblyBuilder>().ToConstant(asmBuilder);
                    kernel.Bind<ISourceTypeFactory>().ToConstructor(x => new SourceTypeFactory());
                    kernel.Bind<ISource>().ToConstructor(x => new ConcreteSource());
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        Stream stream = dbContext.Streams.Single(x => x.Schema.Database.Server.ServerName == DatabaseConstants.TEST_SERVER_NAME && x.Schema.Database.DatabaseName == DatabaseConstants.MASTER_DATABASE_NAME && x.StreamName == streamName);
                        Assert.AreEqual(streamName, stream.StreamName);
                        Assert.IsTrue(stream.IsActive);
                        Assert.AreEqual(eql.Replace('\n', '\0').Trim(), stream.Query);
                        StreamColumn[] projectionColumns = dbContext.StreamColumns.Where(x => x.ServerId == stream.ServerId
                                            && x.DatabaseId == stream.DatabaseId
                                            && x.SchemaId == stream.SchemaId
                                            && x.StreamId == stream.StreamId).ToArray();

                        Assert.IsTrue(projectionColumns.Any(x => x.ColumnName == "c1" && x.ColumnType == typeof(string).AssemblyQualifiedName));
                        Assert.IsTrue(projectionColumns.Any(x => x.ColumnName == "c2" && x.ColumnType == typeof(string).AssemblyQualifiedName));
                        Assert.IsTrue(projectionColumns.Any(x => x.ColumnName == "numeroXXX" && x.ColumnType == typeof(int).AssemblyQualifiedName));
                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el rol de base de datos '{streamName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Creates a stream with status off.
        /// </summary>
        [TestMethod]
        public void CreateStreamWithStatusOff()
        {
            string streamName = "newStream";
            string sourceName = "newSource";
            string eql = "cross " +
                                   $@"JOIN {DatabaseConstants.INPUT_SOURCE_NAME} as t1 WHERE t1.PrimaryAccountNumber == ""9999941616073663_1"" " +
                                   $@"WITH {DatabaseConstants.INPUT_SOURCE_NAME} as t2 WHERE t2.PrimaryAccountNumber == ""9999941616073663_2"" " +
                                   "ON t1.AcquiringInstitutionIdentificationCode == t2.AcquiringInstitutionIdentificationCode " +
                                   "TIMEOUT '00:00:02' " +
                                   $"SELECT (string)t1.PrimaryAccountNumber as c1, t2.PrimaryAccountNumber as c2, 1 as numeroXXX into {sourceName} ";

            string command = $"create source {sourceName} (c1 string(4000), c2 string(4000), numeroXXX int); create stream {streamName} {{ {eql} }} with status = off";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    SpaceAssemblyBuilder sasmBuilder = new SpaceAssemblyBuilder("Test");
                    AssemblyBuilder asmBuilder = sasmBuilder.CreateAssemblyBuilder();
                    SpaceModuleBuilder smodBuilder = new SpaceModuleBuilder(asmBuilder);
                    smodBuilder.CreateModuleBuilder();
                    kernel.Bind<AssemblyBuilder>().ToConstant(asmBuilder);
                    kernel.Bind<ISourceTypeFactory>().ToConstructor(x => new SourceTypeFactory());
                    kernel.Bind<ISource>().ToConstructor(x => new ConcreteSource());
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        Stream stream = dbContext.Streams.Single(x => x.Schema.Database.Server.ServerName == DatabaseConstants.TEST_SERVER_NAME && x.Schema.Database.DatabaseName == DatabaseConstants.MASTER_DATABASE_NAME && x.StreamName == streamName);
                        Assert.AreEqual(streamName, stream.StreamName);
                        Assert.IsFalse(stream.IsActive);
                        Assert.AreEqual(eql.Replace('\n', '\0').Trim(), stream.Query);
                        StreamColumn[] projectionColumns = dbContext.StreamColumns.Where(x => x.ServerId == stream.ServerId
                                            && x.DatabaseId == stream.DatabaseId
                                            && x.SchemaId == stream.SchemaId
                                            && x.StreamId == stream.StreamId).ToArray();

                        Assert.IsTrue(projectionColumns.Any(x => x.ColumnName == "c1" && x.ColumnType == typeof(string).AssemblyQualifiedName));
                        Assert.IsTrue(projectionColumns.Any(x => x.ColumnName == "c2" && x.ColumnType == typeof(string).AssemblyQualifiedName));
                        Assert.IsTrue(projectionColumns.Any(x => x.ColumnName == "numeroXXX" && x.ColumnType == typeof(int).AssemblyQualifiedName));
                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el rol de base de datos '{streamName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        #endregion create stream

        #endregion create

        #region alter

        #region alter login

        /// <summary>
        /// Alters the login name.
        /// </summary>
        [TestMethod]
        public void AlterLoginName()
        {
            string loginName = "login1";
            string newName = "newLogin";
            string password = "oldPassword";
            string command = $"create login {loginName} with password = \"{password}\"; alter login {loginName} with name = {newName}";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        Login login = dbContext.Logins.Single(x => x.LoginName == newName);
                        Assert.AreEqual(newName, login.LoginName);
                        Assert.AreEqual(password, login.LoginPassword);
                        tran.Rollback();
                    }
                    catch
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el login '{loginName}'.");
                    }
                }
            }
        }

        /// <summary>
        /// Alters the password of the login.
        /// </summary>
        [TestMethod]
        public void AlterLoginPassword()
        {
            string loginName = "login1";
            string oldPassword = "oldPassword";
            string newPassword = "newPassword";
            string command = $"create login {loginName} with password = \"{oldPassword}\"; alter login {loginName} with password = \"{newPassword}\"";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        Login login = dbContext.Logins.Single(x => x.LoginName == loginName);
                        Assert.AreEqual(loginName, login.LoginName);
                        Assert.AreEqual(newPassword, login.LoginPassword);
                        tran.Rollback();
                    }
                    catch
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el login '{loginName}'.");
                    }
                }
            }
        }

        /// <summary>
        /// Changes the status of the login to on.
        /// </summary>
        [TestMethod]
        public void AlterLoginWithStatusOn()
        {
            string loginName = "login1";
            string password = "pass1234";
            string command = $"create login {loginName} with password = \"{password}\", status = off; alter login {loginName} with status = on";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        Login login = dbContext.Logins.Single(x => x.LoginName == loginName);
                        Assert.AreEqual(loginName, login.LoginName);
                        Assert.AreEqual(password, login.LoginPassword);
                        Assert.IsTrue(login.IsActive);
                        tran.Rollback();
                    }
                    catch
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el login '{loginName}'.");
                    }
                }
            }
        }

        /// <summary>
        /// Changes the status of the login to off.
        /// </summary>
        [TestMethod]
        public void AlterLoginWithStatusOff()
        {
            string loginName = "login1";
            string password = "pass1234";
            string command = $"create login {loginName} with password = \"{password}\", status = on; alter login {loginName} with status = off";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        Login login = dbContext.Logins.Single(x => x.LoginName == loginName);
                        Assert.AreEqual(loginName, login.LoginName);
                        Assert.AreEqual(password, login.LoginPassword);
                        Assert.IsFalse(login.IsActive);
                        tran.Rollback();
                    }
                    catch
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el login '{loginName}'.");
                    }
                }
            }
        }

        /// <summary>
        /// Alters the default database of the login.
        /// </summary>
        [TestMethod]
        public void AlterLoginWithDefaultDatabase()
        {
            string loginName = "login1";
            string password = "pass1234";
            string oldDatabaseName = DatabaseConstants.MASTER_DATABASE_NAME;
            string newDatabaseName = DatabaseConstants.TEST_DATABASE_NAME;
            string command = $"create login {loginName} with password = \"{password}\", default_database = {oldDatabaseName}; alter login {loginName} with default_database = {newDatabaseName}";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        Login login = dbContext.Logins.Single(x => x.LoginName == loginName);
                        Assert.AreEqual(loginName, login.LoginName);
                        Assert.AreEqual(password, login.LoginPassword);
                        Assert.AreEqual(newDatabaseName, login.Database.DatabaseName);
                        tran.Rollback();
                    }
                    catch
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el login '{loginName}'.");
                    }
                }
            }
        }

        /// <summary>
        /// Alters the password, default database and status to 'on' of the login.
        /// </summary>
        [TestMethod]
        public void AlterLoginWithPasswordDefaultDatabaseAndStatusOn()
        {
            string loginName = "login1";
            string oldPassword = "pass";
            string newPassword = "pass1234";
            string oldDatabaseName = DatabaseConstants.MASTER_DATABASE_NAME;
            string newDatabaseName = DatabaseConstants.TEST_DATABASE_NAME;
            string command = $"create login {loginName} with password = \"{oldPassword}\", default_database = {oldDatabaseName}, status = off; alter login {loginName} with password = \"{newPassword}\", default_database = {newDatabaseName}, status = on";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        Login login = dbContext.Logins.Single(x => x.LoginName == loginName);
                        Assert.AreEqual(loginName, login.LoginName);
                        Assert.AreEqual(newPassword, login.LoginPassword);
                        Assert.AreEqual(newDatabaseName, login.Database.DatabaseName);
                        Assert.IsTrue(login.IsActive);
                        tran.Rollback();
                    }
                    catch
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el login '{loginName}'.");
                    }
                }
            }
        }

        /// <summary>
        /// Alters the password, default database and status to 'off' of the login.
        /// </summary>
        [TestMethod]
        public void AlterLoginWithPasswordDefaultDatabaseAndStatusOff()
        {
            string loginName = "login1";
            string oldPassword = "pass";
            string newPassword = "pass1234";
            string oldDatabaseName = DatabaseConstants.MASTER_DATABASE_NAME;
            string newDatabaseName = DatabaseConstants.TEST_DATABASE_NAME;
            string command = $"create login {loginName} with password = \"{oldPassword}\", default_database = {oldDatabaseName}, status = on; alter login {loginName} with password = \"{newPassword}\", default_database = {newDatabaseName}, status = off";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        Login login = dbContext.Logins.Single(x => x.LoginName == loginName);
                        Assert.AreEqual(loginName, login.LoginName);
                        Assert.AreEqual(newPassword, login.LoginPassword);
                        Assert.AreEqual(newDatabaseName, login.Database.DatabaseName);
                        Assert.IsFalse(login.IsActive);
                        tran.Rollback();
                    }
                    catch
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el login '{loginName}'.");
                    }
                }
            }
        }

        #endregion alter login

        #region alter database

        /// <summary>
        /// Alters the database name.
        /// </summary>
        [TestMethod]
        public void AlterDatabaseName()
        {
            string oldDatabaseName = "oldDatabase";
            string newDatabaseName = "newDatabase";
            string command = $"create database {oldDatabaseName}; alter database {oldDatabaseName} with name = {newDatabaseName}";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        Space.Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == newDatabaseName);
                        Assert.AreEqual(newDatabaseName, database.DatabaseName);
                        tran.Rollback();
                    }
                    catch
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear la base de datos '{newDatabaseName}'.");
                    }
                }
            }
        }

        /// <summary>
        /// Alters the database status to on.
        /// </summary>
        [TestMethod]
        public void AlterDatabaseWithStatusOn()
        {
            string databaseName = "newDatabase";
            string command = $"create database {databaseName} with status = off; alter database {databaseName} with status = on";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        Space.Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == databaseName);
                        Assert.AreEqual(databaseName, database.DatabaseName);
                        Assert.IsTrue(database.IsActive);
                        tran.Rollback();
                    }
                    catch
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear la base de datos '{databaseName}'.");
                    }
                }
            }
        }

        /// <summary>
        /// Alters the database status to off.
        /// </summary>
        [TestMethod]
        public void AlterDatabaseWithStatusOff()
        {
            string databaseName = "newDatabase";
            string command = $"create database {databaseName} with status = on; alter database {databaseName} with status = off";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        Space.Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == databaseName);
                        Assert.AreEqual(databaseName, database.DatabaseName);
                        Assert.IsFalse(database.IsActive);
                        tran.Rollback();
                    }
                    catch
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear la base de datos '{databaseName}'.");
                    }
                }
            }
        }

        #endregion alter database

        #region alter user

        /// <summary>
        /// Alters the user name of an existing role.
        /// </summary>
        [TestMethod]
        public void AlterUserWithName()
        {
            string oldUserName = "oldUser";
            string newUserName = "newUser";
            string loginUserName = DatabaseConstants.ADMIN_LOGIN_1_NAME;
            string command = $"use {DatabaseConstants.TEST_DATABASE_NAME}; create user {oldUserName} with login = {loginUserName}; alter user {oldUserName} with name = {newUserName}";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.DbUsrName == newUserName);
                        Assert.AreEqual(newUserName, user.DbUsrName);
                        Assert.AreEqual(loginUserName, user.Login.LoginName);
                        Assert.IsTrue(user.IsActive);
                        tran.Rollback();
                    }
                    catch
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear la base de datos '{newUserName}'.");
                    }
                }
            }
        }

        /// <summary>
        /// Alters the login mapped of the user.
        /// </summary>
        [TestMethod]
        public void AlterUserWithLogin()
        {
            string userName = "newUser";
            string oldLoginName = DatabaseConstants.ADMIN_LOGIN_1_NAME;
            string newLoginName = DatabaseConstants.NORMAL_LOGIN_2_NAME;
            string command = $"use {DatabaseConstants.TEST_DATABASE_NAME}; create user {userName} with login = {oldLoginName}; alter user {userName} with login = {newLoginName}";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.DbUsrName == userName);
                        Assert.AreEqual(userName, user.DbUsrName);
                        Assert.AreEqual(newLoginName, user.Login.LoginName);
                        Assert.IsTrue(user.IsActive);
                        tran.Rollback();
                    }
                    catch
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear la base de datos '{userName}'.");
                    }
                }
            }
        }

        /// <summary>
        /// Alters the user mapped login and default schema.
        /// </summary>
        [TestMethod]
        public void AlterUserWithLoginDefaultSchema()
        {
            string userName = "newUser";
            string oldLoginName = DatabaseConstants.ADMIN_LOGIN_1_NAME;
            string newLoginName = DatabaseConstants.ADMIN_LOGIN_2_NAME;
            string oldSchemaName = "Schema1";
            string newSchemaName = "Schema2";
            string command = $"use {DatabaseConstants.TEST_DATABASE_NAME}; create schema {oldSchemaName}; create schema {newSchemaName}; create user {userName} with login = {oldLoginName}, default_schema = {oldSchemaName}; alter user {userName} with login = {newLoginName}, default_schema = {newSchemaName}";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.DbUsrName == userName);
                        Assert.AreEqual(userName, user.DbUsrName);
                        Assert.AreEqual(newLoginName, user.Login.LoginName);
                        Assert.AreEqual(newSchemaName, user.DefaultSchema.SchemaName);
                        Assert.IsTrue(user.IsActive);
                        tran.Rollback();
                    }
                    catch
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear la base de datos '{userName}'.");
                    }
                }
            }
        }

        /// <summary>
        /// Alters the user status to on.
        /// </summary>
        [TestMethod]
        public void AlterUserWithStatusOn()
        {
            string userName = "newUser";
            string loginUserName = DatabaseConstants.ADMIN_LOGIN_1_NAME;
            string command = $"use {DatabaseConstants.TEST_DATABASE_NAME}; create user {userName} with login = {loginUserName}, status = off; alter user {userName} with status = on";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.DbUsrName == userName);
                        Assert.AreEqual(userName, user.DbUsrName);
                        Assert.AreEqual(loginUserName, user.Login.LoginName);
                        Assert.IsTrue(user.IsActive);
                        tran.Rollback();
                    }
                    catch
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear la base de datos '{userName}'.");
                    }
                }
            }
        }

        /// <summary>
        /// Alters the user status to off.
        /// </summary>
        [TestMethod]
        public void AlterUserWithStatusOff()
        {
            string userName = "newUser";
            string loginUserName = DatabaseConstants.ADMIN_LOGIN_1_NAME;
            string command = $"use {DatabaseConstants.TEST_DATABASE_NAME}; create user {userName} with login = {loginUserName}, status = on; alter user {userName} with status = off";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.DbUsrName == userName);
                        Assert.AreEqual(userName, user.DbUsrName);
                        Assert.AreEqual(loginUserName, user.Login.LoginName);
                        Assert.IsFalse(user.IsActive);
                        tran.Rollback();
                    }
                    catch
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear la base de datos '{userName}'.");
                    }
                }
            }
        }

        /// <summary>
        /// Alters the user mapped login, default schema and status to off of the user.
        /// </summary>
        [TestMethod]
        public void AlterUserWithLoginDefaultSchemaLoginStatusOff()
        {
            string userName = "newUser";
            string oldLoginName = DatabaseConstants.ADMIN_LOGIN_1_NAME;
            string newLoginName = DatabaseConstants.ADMIN_LOGIN_2_NAME;
            string oldSchemaName = "Schema1";
            string newSchemaName = "Schema2";
            string command = $"use {DatabaseConstants.TEST_DATABASE_NAME}; create schema {oldSchemaName}; create schema {newSchemaName}; create user {userName} with login = {oldLoginName}, default_schema = {oldSchemaName}, status = on; alter user {userName} with login = {newLoginName}, default_schema = {newSchemaName}, status = off";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.DbUsrName == userName);
                        Assert.AreEqual(userName, user.DbUsrName);
                        Assert.AreEqual(newLoginName, user.Login.LoginName);
                        Assert.AreEqual(newSchemaName, user.DefaultSchema.SchemaName);
                        Assert.IsFalse(user.IsActive);
                        tran.Rollback();
                    }
                    catch
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear la base de datos '{userName}'.");
                    }
                }
            }
        }

        /// <summary>
        /// Alters the user mapped login, default schema and status to on of the user.
        /// </summary>
        [TestMethod]
        public void AlterUserWithDefaultSchemaLoginStatusOn()
        {
            string userName = "newUser";
            string oldLoginName = DatabaseConstants.ADMIN_LOGIN_1_NAME;
            string newLoginName = DatabaseConstants.ADMIN_LOGIN_2_NAME;
            string oldSchemaName = "Schema1";
            string newSchemaName = "Schema2";
            string command = $"use {DatabaseConstants.TEST_DATABASE_NAME}; create schema {oldSchemaName}; create schema {newSchemaName}; create user {userName} with login = {oldLoginName}, default_schema = {oldSchemaName}, status = off; alter user {userName} with login = {newLoginName}, default_schema = {newSchemaName}, status = on";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.DbUsrName == userName);
                        Assert.AreEqual(userName, user.DbUsrName);
                        Assert.AreEqual(newLoginName, user.Login.LoginName);
                        Assert.AreEqual(newSchemaName, user.DefaultSchema.SchemaName);
                        Assert.IsTrue(user.IsActive);
                        tran.Rollback();
                    }
                    catch
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear la base de datos '{userName}'.");
                    }
                }
            }
        }

        #endregion alter user

        #region alter role

        /// <summary>
        /// Alters the role name of an existing role.
        /// </summary>
        [TestMethod]
        public void AlterRoleName()
        {
            string oldRoleName = DatabaseConstants.INEXISTENT_ROLE_NAME_1;
            string newRoleName = DatabaseConstants.INEXISTENT_ROLE_NAME_2;
            string command = $"create role {oldRoleName}; alter role {oldRoleName} with name = {newRoleName}";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        DatabaseRole role = dbContext.DatabaseRoles.Single(x => x.Database.Server.ServerName == DatabaseConstants.TEST_SERVER_NAME && x.Database.DatabaseName == DatabaseConstants.MASTER_DATABASE_NAME && x.DbRoleName == newRoleName);
                        Assert.AreEqual(newRoleName, role.DbRoleName);
                        Assert.IsTrue(role.IsActive);
                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el rol de base de datos '{newRoleName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Alters the role status to on.
        /// </summary>
        [TestMethod]
        public void AlterRoleWithStatusOn()
        {
            string roleName = DatabaseConstants.INEXISTENT_ROLE_NAME_1;
            string command = $"create role {roleName} with status = off; alter role {roleName} with status = on";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        DatabaseRole role = dbContext.DatabaseRoles.Single(x => x.Database.Server.ServerName == DatabaseConstants.TEST_SERVER_NAME && x.Database.DatabaseName == DatabaseConstants.MASTER_DATABASE_NAME && x.DbRoleName == roleName);
                        Assert.AreEqual(roleName, role.DbRoleName);
                        Assert.IsTrue(role.IsActive);
                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al editar el rol de base de datos '{roleName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Alters the role status to off.
        /// </summary>
        [TestMethod]
        public void AlterRoleWithStatusOff()
        {
            string roleName = DatabaseConstants.INEXISTENT_ROLE_NAME_1;
            string command = $"create role {roleName} with status = on; alter role {roleName} with status = off";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        DatabaseRole role = dbContext.DatabaseRoles.Single(x => x.Database.Server.ServerName == DatabaseConstants.TEST_SERVER_NAME && x.Database.DatabaseName == DatabaseConstants.MASTER_DATABASE_NAME && x.DbRoleName == roleName);
                        Assert.AreEqual(roleName, role.DbRoleName);
                        Assert.IsFalse(role.IsActive);
                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al editar el rol de base de datos '{roleName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Alters a role adding a user.
        /// </summary>
        [TestMethod]
        public void AlterRoleAddUser()
        {
            string roleName = DatabaseConstants.INEXISTENT_ROLE_NAME_1;
            string userName = DatabaseConstants.NORMAL_USER_2_NAME;
            string command = $"create role {roleName}; alter role {roleName} with add = {userName}";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        DatabaseRole role = dbContext.DatabaseRoles.Single(x => x.Database.Server.ServerName == DatabaseConstants.TEST_SERVER_NAME && x.Database.DatabaseName == DatabaseConstants.MASTER_DATABASE_NAME && x.DbRoleName == roleName);
                        Assert.AreEqual(roleName, role.DbRoleName);
                        Assert.IsTrue(role.IsActive);

                        DatabaseUser user = role.DatabaseUsers.Single(x => x.ServerId == role.ServerId && x.DatabaseId == role.DatabaseId && x.DbUsrName == userName);
                        Assert.AreEqual(userName, user.DbUsrName);
                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al editar el rol de base de datos '{roleName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Alters a role adding multiple users.
        /// </summary>
        [TestMethod]
        public void AlterRoleAddUsers()
        {
            string roleName = DatabaseConstants.INEXISTENT_ROLE_NAME_1;
            string userName1 = DatabaseConstants.NORMAL_USER_2_NAME;
            string userName2 = DatabaseConstants.NORMAL_USER_1_NAME;
            string userName3 = DatabaseConstants.DBO_USER_NAME;
            string command = $"Create role {roleName}; alter role {roleName} with add = {userName1} {userName2} {userName3}";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        DatabaseRole role = dbContext.DatabaseRoles.Single(x => x.Database.Server.ServerName == DatabaseConstants.TEST_SERVER_NAME && x.Database.DatabaseName == DatabaseConstants.MASTER_DATABASE_NAME && x.DbRoleName == roleName);
                        Assert.AreEqual(roleName, role.DbRoleName);
                        Assert.IsTrue(role.IsActive);

                        DatabaseUser user = role.DatabaseUsers.Single(x => x.ServerId == role.ServerId && x.DatabaseId == role.DatabaseId && x.DbUsrName == userName1);
                        Assert.AreEqual(userName1, user.DbUsrName);
                        user = role.DatabaseUsers.Single(x => x.ServerId == role.ServerId && x.DatabaseId == role.DatabaseId && x.DbUsrName == userName2);
                        Assert.AreEqual(userName2, user.DbUsrName);
                        user = role.DatabaseUsers.Single(x => x.ServerId == role.ServerId && x.DatabaseId == role.DatabaseId && x.DbUsrName == userName3);
                        Assert.AreEqual(userName3, user.DbUsrName);
                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al editar el rol de base de datos '{roleName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Alters a role removing a user.
        /// </summary>
        [TestMethod]
        public void AlterRoleRemoveUser()
        {
            string roleName = DatabaseConstants.INEXISTENT_ROLE_NAME_1;
            string userName = DatabaseConstants.NORMAL_USER_2_NAME;
            string command = $"create role {roleName}; alter role {roleName} with add = {userName}; alter role {roleName} with remove = {userName}";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        DatabaseRole role = dbContext.DatabaseRoles.Single(x => x.Database.Server.ServerName == DatabaseConstants.TEST_SERVER_NAME && x.Database.DatabaseName == DatabaseConstants.MASTER_DATABASE_NAME && x.DbRoleName == roleName);
                        Assert.AreEqual(roleName, role.DbRoleName);
                        Assert.IsTrue(role.IsActive);

                        bool exists = role.DatabaseUsers.Any(x => x.ServerId == role.ServerId && x.DatabaseId == role.DatabaseId && x.DbUsrName == userName);
                        Assert.IsFalse(exists);
                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al editar el rol de base de datos '{roleName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Alters a role removing multiple users.
        /// </summary>
        [TestMethod]
        public void AlterRoleRemoveUsers()
        {
            string roleName = DatabaseConstants.INEXISTENT_ROLE_NAME_1;
            string userName1 = DatabaseConstants.NORMAL_USER_2_NAME;
            string userName2 = DatabaseConstants.NORMAL_USER_1_NAME;
            string userName3 = DatabaseConstants.DBO_USER_NAME;
            string command = $"Create role {roleName}; alter role {roleName} with add = {userName1} {userName2} {userName3}; alter role {roleName} with remove = {userName1} {userName2} {userName3}";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        DatabaseRole role = dbContext.DatabaseRoles.Single(x => x.Database.Server.ServerName == DatabaseConstants.TEST_SERVER_NAME && x.Database.DatabaseName == DatabaseConstants.MASTER_DATABASE_NAME && x.DbRoleName == roleName);
                        Assert.AreEqual(roleName, role.DbRoleName);
                        Assert.IsTrue(role.IsActive);

                        bool exists = role.DatabaseUsers.Any(x => x.ServerId == role.ServerId && x.DatabaseId == role.DatabaseId && x.DbUsrName == userName1);
                        exists = exists || role.DatabaseUsers.Any(x => x.ServerId == role.ServerId && x.DatabaseId == role.DatabaseId && x.DbUsrName == userName2);
                        exists = exists || role.DatabaseUsers.Any(x => x.ServerId == role.ServerId && x.DatabaseId == role.DatabaseId && x.DbUsrName == userName3);
                        Assert.IsFalse(exists);
                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al editar el rol de base de datos '{roleName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        #endregion alter role

        #region alter schema

        /// <summary>
        /// Alters a schema name.
        /// </summary>
        [TestMethod]
        public void AlterSchemaWithName()
        {
            string oldSchemaName = "oldSchema";
            string newSchemaName = "newSchema";
            string command = $"create schema {oldSchemaName}; alter schema {oldSchemaName} with name = {newSchemaName}";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        Schema schema = dbContext.Schemas.Single(x => x.SchemaName == newSchemaName);
                        Assert.AreEqual(newSchemaName, schema.SchemaName);
                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();

                        Assert.Fail($"Error al crear el rol de base de datos '{newSchemaName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        #endregion alter schema

        #region alter source

        /// <summary>
        /// Alters the name of an existing source.
        /// </summary>
        [TestMethod]
        public void AlterSourceWithName()
        {
            string oldSourceName = "oldSourceName";
            string newSourceName = "newSource";
            string command = $"create source {oldSourceName} (column1 string(4000), column2 int, column3 double); alter source {oldSourceName} with name = {newSourceName}";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    SpaceAssemblyBuilder sasmBuilder = new SpaceAssemblyBuilder("Test");
                    AssemblyBuilder asmBuilder = sasmBuilder.CreateAssemblyBuilder();
                    SpaceModuleBuilder smodBuilder = new SpaceModuleBuilder(asmBuilder);
                    smodBuilder.CreateModuleBuilder();
                    kernel.Bind<AssemblyBuilder>().ToConstant(asmBuilder);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        Source source = dbContext.Sources.Single(x => x.SourceName == newSourceName);
                        Assert.AreEqual(newSourceName, source.SourceName);
                        Assert.IsTrue(dbContext.SourceColumns.Any(x => x.ColumnName == "column1"));
                        Assert.IsTrue(dbContext.SourceColumns.Any(x => x.ColumnName == "column2"));
                        Assert.IsTrue(dbContext.SourceColumns.Any(x => x.ColumnName == "column3"));
                        Assert.IsTrue(source.IsActive);
                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el rol de base de datos '{newSourceName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Alters the source structure adding columns.
        /// </summary>
        [TestMethod]
        public void AlterSourceAddColumns()
        {
            string sourceName = "oldSourceName";
            string command = $@"create source {sourceName} (column1 string(4000), column2 int, column3 double); 
                                alter source {sourceName} add (columnX string(4000), columnY int, columnZ double)";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    SpaceAssemblyBuilder sasmBuilder = new SpaceAssemblyBuilder("Test");
                    AssemblyBuilder asmBuilder = sasmBuilder.CreateAssemblyBuilder();
                    SpaceModuleBuilder smodBuilder = new SpaceModuleBuilder(asmBuilder);
                    smodBuilder.CreateModuleBuilder();
                    kernel.Bind<AssemblyBuilder>().ToConstant(asmBuilder);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        Source source = dbContext.Sources.Single(x => x.SourceName == sourceName);
                        Assert.AreEqual(sourceName, source.SourceName);
                        Assert.IsTrue(dbContext.SourceColumns.Any(x => x.ColumnName == "column1"));
                        Assert.IsTrue(dbContext.SourceColumns.Any(x => x.ColumnName == "column2"));
                        Assert.IsTrue(dbContext.SourceColumns.Any(x => x.ColumnName == "column3"));
                        Assert.IsTrue(dbContext.SourceColumns.Any(x => x.ColumnName == "columnX"));
                        Assert.IsTrue(dbContext.SourceColumns.Any(x => x.ColumnName == "columnY"));
                        Assert.IsTrue(dbContext.SourceColumns.Any(x => x.ColumnName == "columnZ"));
                        Assert.IsTrue(source.IsActive);
                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el rol de base de datos '{sourceName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Alters the source structure removing columns.
        /// </summary>
        [TestMethod]
        public void AlterSourceRemoveColumns()
        {
            string sourceName = "oldSourceName";
            string command = $@"create source {sourceName} (column1 string(4000), column2 int, column3 double, columnX string(4000), columnY int, columnZ double); 
                                alter source {sourceName} remove (columnX, columnY, columnZ)";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    SpaceAssemblyBuilder sasmBuilder = new SpaceAssemblyBuilder("Test");
                    AssemblyBuilder asmBuilder = sasmBuilder.CreateAssemblyBuilder();
                    SpaceModuleBuilder smodBuilder = new SpaceModuleBuilder(asmBuilder);
                    smodBuilder.CreateModuleBuilder();
                    kernel.Bind<AssemblyBuilder>().ToConstant(asmBuilder);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        Source source = dbContext.Sources.Single(x => x.SourceName == sourceName);
                        Assert.AreEqual(sourceName, source.SourceName);
                        Assert.IsTrue(dbContext.SourceColumns.Any(x => x.ColumnName == "column1"));
                        Assert.IsTrue(dbContext.SourceColumns.Any(x => x.ColumnName == "column2"));
                        Assert.IsTrue(dbContext.SourceColumns.Any(x => x.ColumnName == "column3"));
                        Assert.IsFalse(dbContext.SourceColumns.Any(x => x.ColumnName == "columnX"));
                        Assert.IsFalse(dbContext.SourceColumns.Any(x => x.ColumnName == "columnY"));
                        Assert.IsFalse(dbContext.SourceColumns.Any(x => x.ColumnName == "columnZ"));
                        Assert.IsTrue(source.IsActive);
                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear la fuente de base de datos '{sourceName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Alters the source structure trying to remove all columns, however this is not allowed.
        /// </summary>
        [TestMethod]
        public void AlterSourceRemoveAllColumns()
        {
            string sourceName = "oldSourceName";
            string command = $@"create source {sourceName} (columnX string, columnY int, columnZ double); 
                                alter source {sourceName} remove columnX string, columnY int, columnZ double";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    try
                    {
                        FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                        tran.Rollback();
                        Assert.Fail("Dejó eliminar todas las columnas de la fuente");
                    }
                    catch (Exception)
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Alters a source changing its status to on.
        /// </summary>
        [TestMethod]
        public void AlterSourceWithStatusOn()
        {
            string sourceName = "newSource";
            string command = $"create source {sourceName} (column1 string(4000), column2 int, column3 double) with status = off; alter source {sourceName} with status = on";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    SpaceAssemblyBuilder sasmBuilder = new SpaceAssemblyBuilder("Test");
                    AssemblyBuilder asmBuilder = sasmBuilder.CreateAssemblyBuilder();
                    SpaceModuleBuilder smodBuilder = new SpaceModuleBuilder(asmBuilder);
                    smodBuilder.CreateModuleBuilder();
                    kernel.Bind<AssemblyBuilder>().ToConstant(asmBuilder);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        Source source = dbContext.Sources.Single(x => x.SourceName == sourceName);
                        Assert.AreEqual(sourceName, source.SourceName);
                        Assert.IsTrue(source.IsActive);
                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el rol de base de datos '{sourceName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Alters a source changing its status to off.
        /// </summary>
        [TestMethod]
        public void AlterSourceWithStatusOff()
        {
            string sourceName = "newSource";
            string command = $"create source {sourceName} (column1 string(4000), column2 int, column3 double) with status = on; alter source {sourceName} with status = off";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    SpaceAssemblyBuilder sasmBuilder = new SpaceAssemblyBuilder("Test");
                    AssemblyBuilder asmBuilder = sasmBuilder.CreateAssemblyBuilder();
                    SpaceModuleBuilder smodBuilder = new SpaceModuleBuilder(asmBuilder);
                    smodBuilder.CreateModuleBuilder();
                    kernel.Bind<AssemblyBuilder>().ToConstant(asmBuilder);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        Source source = dbContext.Sources.Single(x => x.SourceName == sourceName);
                        Assert.AreEqual(sourceName, source.SourceName);
                        Assert.IsFalse(source.IsActive);
                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el rol de base de datos '{sourceName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Alters a source changing its persistent flag to on.
        /// </summary>
        [TestMethod]
        public void AlterSourceWithPersistentOn()
        {
            string sourceName = "newSource";
            string command = $"create source {sourceName} (column1 string(4000), column2 int, column3 double) with persistent = off; alter source {sourceName} with persistent = on";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    SpaceAssemblyBuilder sasmBuilder = new SpaceAssemblyBuilder("Test");
                    AssemblyBuilder asmBuilder = sasmBuilder.CreateAssemblyBuilder();
                    SpaceModuleBuilder smodBuilder = new SpaceModuleBuilder(asmBuilder);
                    smodBuilder.CreateModuleBuilder();
                    kernel.Bind<AssemblyBuilder>().ToConstant(asmBuilder);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        Source source = dbContext.Sources.Single(x => x.SourceName == sourceName);
                        Assert.AreEqual(sourceName, source.SourceName);
                        Assert.IsTrue(dbContext.SourceColumns.Any(x => x.ColumnName == "column1"));
                        Assert.IsTrue(dbContext.SourceColumns.Any(x => x.ColumnName == "column2"));
                        Assert.IsTrue(dbContext.SourceColumns.Any(x => x.ColumnName == "column3"));
                        Assert.IsTrue(source.IsActive);
                        Assert.IsTrue(source.Persistent);
                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el rol de base de datos '{sourceName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Alters a source changing its persistent flag to on.
        /// </summary>
        [TestMethod]
        public void AlterSourceWithPersistentOff()
        {
            string sourceName = "newSource";
            string command = $"create source {sourceName} (column1 string(4000), column2 int, column3 double) with persistent = on; alter source {sourceName} with persistent = off";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    SpaceAssemblyBuilder sasmBuilder = new SpaceAssemblyBuilder("Test");
                    AssemblyBuilder asmBuilder = sasmBuilder.CreateAssemblyBuilder();
                    SpaceModuleBuilder smodBuilder = new SpaceModuleBuilder(asmBuilder);
                    smodBuilder.CreateModuleBuilder();
                    kernel.Bind<AssemblyBuilder>().ToConstant(asmBuilder);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        Source source = dbContext.Sources.Single(x => x.SourceName == sourceName);
                        Assert.AreEqual(sourceName, source.SourceName);
                        Assert.IsTrue(dbContext.SourceColumns.Any(x => x.ColumnName == "column1"));
                        Assert.IsTrue(dbContext.SourceColumns.Any(x => x.ColumnName == "column2"));
                        Assert.IsTrue(dbContext.SourceColumns.Any(x => x.ColumnName == "column3"));
                        Assert.IsTrue(source.IsActive);
                        Assert.IsFalse(source.Persistent);
                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el rol de base de datos '{sourceName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Alters a source changing its cache size.
        /// </summary>
        [TestMethod]
        public void AlterSourceWithCacheSize()
        {
            string sourceName = "newSource";
            string command = $"create source {sourceName} (column1 string(4000), column2 int, column3 double) with status = on, cache_size = 100; alter source {sourceName} with status = off, cache_size = 200";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    SpaceAssemblyBuilder sasmBuilder = new SpaceAssemblyBuilder("Test");
                    AssemblyBuilder asmBuilder = sasmBuilder.CreateAssemblyBuilder();
                    SpaceModuleBuilder smodBuilder = new SpaceModuleBuilder(asmBuilder);
                    smodBuilder.CreateModuleBuilder();
                    kernel.Bind<AssemblyBuilder>().ToConstant(asmBuilder);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        Source source = dbContext.Sources.Single(x => x.SourceName == sourceName);
                        Assert.AreEqual(sourceName, source.SourceName);
                        Assert.IsTrue(dbContext.SourceColumns.Any(x => x.ColumnName == "column1"));
                        Assert.IsTrue(dbContext.SourceColumns.Any(x => x.ColumnName == "column2"));
                        Assert.IsTrue(dbContext.SourceColumns.Any(x => x.ColumnName == "column3"));
                        Assert.AreEqual<uint>((uint)source.CacheSize, 200);
                        Assert.IsFalse(source.IsActive);
                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el rol de base de datos '{sourceName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Alters a source changing its cache durability.
        /// </summary>
        [TestMethod]
        public void AlterSourceWithCacheDurability()
        {
            string sourceName = "newSource";
            string command = $"create source {sourceName} (column1 string(4000), column2 int, column3 double) with status = on, cache_durability = 100; alter source {sourceName} with status = off, cache_durability = 70";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    SpaceAssemblyBuilder sasmBuilder = new SpaceAssemblyBuilder("Test");
                    AssemblyBuilder asmBuilder = sasmBuilder.CreateAssemblyBuilder();
                    SpaceModuleBuilder smodBuilder = new SpaceModuleBuilder(asmBuilder);
                    smodBuilder.CreateModuleBuilder();
                    kernel.Bind<AssemblyBuilder>().ToConstant(asmBuilder);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        Source source = dbContext.Sources.Single(x => x.SourceName == sourceName);
                        Assert.AreEqual(sourceName, source.SourceName);
                        Assert.IsTrue(dbContext.SourceColumns.Any(x => x.ColumnName == "column1"));
                        Assert.IsTrue(dbContext.SourceColumns.Any(x => x.ColumnName == "column2"));
                        Assert.IsTrue(dbContext.SourceColumns.Any(x => x.ColumnName == "column3"));
                        Assert.AreEqual<uint>((uint)source.CacheDurability, 70);
                        Assert.IsFalse(source.IsActive);
                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el rol de base de datos '{sourceName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Alters a source changing its cache size and cache durability.
        /// </summary>
        [TestMethod]
        public void AlterSourceWithCacheSizeCacheDurability()
        {
            string sourceName = "newSource";
            string command = $"create source {sourceName} (column1 string(4000), column2 int, column3 double) with status = on, cache_size = 100, cache_durability = 100; alter source {sourceName} with status = off, cache_size = 200, cache_durability = 70";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    SpaceAssemblyBuilder sasmBuilder = new SpaceAssemblyBuilder("Test");
                    AssemblyBuilder asmBuilder = sasmBuilder.CreateAssemblyBuilder();
                    SpaceModuleBuilder smodBuilder = new SpaceModuleBuilder(asmBuilder);
                    smodBuilder.CreateModuleBuilder();
                    kernel.Bind<AssemblyBuilder>().ToConstant(asmBuilder);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        Source source = dbContext.Sources.Single(x => x.SourceName == sourceName);
                        Assert.AreEqual(sourceName, source.SourceName);
                        Assert.IsTrue(dbContext.SourceColumns.Any(x => x.ColumnName == "column1"));
                        Assert.IsTrue(dbContext.SourceColumns.Any(x => x.ColumnName == "column2"));
                        Assert.IsTrue(dbContext.SourceColumns.Any(x => x.ColumnName == "column3"));
                        Assert.AreEqual<uint>((uint)source.CacheSize, 200);
                        Assert.AreEqual<uint>((uint)source.CacheDurability, 70);
                        Assert.IsFalse(source.IsActive);
                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el rol de base de datos '{sourceName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        #endregion alter source

        #region alter stream

        /// <summary>
        /// Alters the name of an existing stream.
        /// </summary>
        [TestMethod]
        public void AlterStreamWithName()
        {
            string oldStreamName = "oldStream";
            string newStreamName = "newStream";
            string sourceName = "newSource";
            string eql = "cross " +
                                   $@"JOIN {DatabaseConstants.INPUT_SOURCE_NAME} as t1 WHERE t1.PrimaryAccountNumber == ""9999941616073663_1"" " +
                                   $@"WITH {DatabaseConstants.INPUT_SOURCE_NAME} as t2 WHERE t2.PrimaryAccountNumber == ""9999941616073663_2"" " +
                                   "ON t1.AcquiringInstitutionIdentificationCode == t2.AcquiringInstitutionIdentificationCode " +
                                   "TIMEOUT '00:00:02' " +
                                   $"SELECT (string)t1.PrimaryAccountNumber as c1, t2.PrimaryAccountNumber as c2, 1 as numeroXXX into {sourceName}";

            string command = $"create source {sourceName} (c1 string(4000), c2 string(4000), numeroXXX int); create stream {oldStreamName} {{ {eql} }}; alter stream {oldStreamName} with name = {newStreamName}";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    SpaceAssemblyBuilder sasmBuilder = new SpaceAssemblyBuilder("Test");
                    AssemblyBuilder asmBuilder = sasmBuilder.CreateAssemblyBuilder();
                    SpaceModuleBuilder smodBuilder = new SpaceModuleBuilder(asmBuilder);
                    smodBuilder.CreateModuleBuilder();
                    kernel.Bind<AssemblyBuilder>().ToConstant(asmBuilder);
                    kernel.Bind<ISourceTypeFactory>().ToConstructor(x => new SourceTypeFactory());
                    kernel.Bind<ISource>().ToConstructor(x => new ConcreteSource());
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        Stream stream = dbContext.Streams.Single(x => x.StreamName == newStreamName);
                        Assert.AreEqual(newStreamName, stream.StreamName);
                        Assert.IsTrue(stream.IsActive);
                        Assert.AreEqual(eql.Replace('\n', '\0').Trim(), stream.Query);
                        StreamColumn[] projectionColumns = dbContext.StreamColumns.Where(x => x.ServerId == stream.ServerId
                                            && x.DatabaseId == stream.DatabaseId
                                            && x.SchemaId == stream.SchemaId
                                            && x.StreamId == stream.StreamId).ToArray();

                        Assert.IsTrue(projectionColumns.Any(x => x.ColumnName == "c1" && x.ColumnType == typeof(string).AssemblyQualifiedName));
                        Assert.IsTrue(projectionColumns.Any(x => x.ColumnName == "c2" && x.ColumnType == typeof(string).AssemblyQualifiedName));
                        Assert.IsTrue(projectionColumns.Any(x => x.ColumnName == "numeroXXX" && x.ColumnType == typeof(int).AssemblyQualifiedName));
                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el rol de base de datos '{newStreamName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Alters the query of a stream removing columns of its projection, i. e. the select statement.
        /// </summary>
        [TestMethod]
        public void AlterStreamRemoveProjectionColumns()
        {
            string oldStreamName = "oldStream";
            string newStreamName = "newStream";
            string sourceName = "newSource";
            string eql = "cross " +
                                   $@"JOIN {DatabaseConstants.INPUT_SOURCE_NAME} as t1 WHERE t1.PrimaryAccountNumber == ""9999941616073663_1"" " +
                                   $@"WITH {DatabaseConstants.INPUT_SOURCE_NAME} as t2 WHERE t2.PrimaryAccountNumber == ""9999941616073663_2"" " +
                                   "ON t1.AcquiringInstitutionIdentificationCode == t2.AcquiringInstitutionIdentificationCode " +
                                   "TIMEOUT '00:00:02' " +
                                   $"SELECT (string)t1.PrimaryAccountNumber as c1, t2.PrimaryAccountNumber as c2, 1 as numeroXXX into {sourceName}";

            string eql2 = "cross " +
                                   $@"JOIN {DatabaseConstants.INPUT_SOURCE_NAME} as t1 WHERE t1.PrimaryAccountNumber == ""9999941616073663_1"" " +
                                   $@"WITH {DatabaseConstants.INPUT_SOURCE_NAME} as t2 WHERE t2.PrimaryAccountNumber == ""9999941616073663_2"" " +
                                   "ON t1.AcquiringInstitutionIdentificationCode == t2.AcquiringInstitutionIdentificationCode " +
                                   "TIMEOUT '00:00:02' " +
                                   $"SELECT (string)t1.PrimaryAccountNumber as c1, 1 as numeroXXX into {sourceName}";

            string command = $"create source {sourceName} (c1 string(4000), c2 string(4000), numeroXXX int); create stream {oldStreamName} {{ {eql} }}; alter stream {oldStreamName} with query = {{ {eql2} }}";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    SpaceAssemblyBuilder sasmBuilder = new SpaceAssemblyBuilder("Test");
                    AssemblyBuilder asmBuilder = sasmBuilder.CreateAssemblyBuilder();
                    SpaceModuleBuilder smodBuilder = new SpaceModuleBuilder(asmBuilder);
                    smodBuilder.CreateModuleBuilder();
                    kernel.Bind<AssemblyBuilder>().ToConstant(asmBuilder);
                    kernel.Bind<ISourceTypeFactory>().ToConstructor(x => new SourceTypeFactory());
                    kernel.Bind<ISource>().ToConstructor(x => new ConcreteSource());
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        Stream stream = dbContext.Streams.Single(x => x.StreamName == oldStreamName);
                        Assert.AreEqual(oldStreamName, stream.StreamName);
                        Assert.IsTrue(stream.IsActive);
                        Assert.AreEqual(eql2.Replace('\n', '\0').Trim(), stream.Query);
                        StreamColumn[] projectionColumns = dbContext.StreamColumns.Where(x => x.ServerId == stream.ServerId
                                            && x.DatabaseId == stream.DatabaseId
                                            && x.SchemaId == stream.SchemaId
                                            && x.StreamId == stream.StreamId).ToArray();

                        Assert.IsTrue(projectionColumns.Any(x => x.ColumnName == "c1" && x.ColumnType == typeof(string).AssemblyQualifiedName));
                        Assert.IsTrue(projectionColumns.Any(x => x.ColumnName == "numeroXXX" && x.ColumnType == typeof(int).AssemblyQualifiedName));
                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el rol de base de datos '{newStreamName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Alters the query of a stream adding columns of its projection, i. e. the select statement.
        /// </summary>
        [TestMethod]
        public void AlterStreamAddProjectionColumns()
        {
            string oldStreamName = "oldStream";
            string newStreamName = "newStream";
            string sourceName = "newSource";
            string eql = "cross " +
                                   $@"JOIN {DatabaseConstants.INPUT_SOURCE_NAME} as t1 WHERE t1.PrimaryAccountNumber == ""9999941616073663_1"" " +
                                   $@"WITH {DatabaseConstants.INPUT_SOURCE_NAME} as t2 WHERE t2.PrimaryAccountNumber == ""9999941616073663_2"" " +
                                   "ON t1.AcquiringInstitutionIdentificationCode == t2.AcquiringInstitutionIdentificationCode " +
                                   "TIMEOUT '00:00:02' " +
                                   $"SELECT (string)t1.PrimaryAccountNumber as c1, t2.PrimaryAccountNumber as c2, 1 as numeroXXX into {sourceName}";

            string eql2 = "cross " +
                                   $@"JOIN {DatabaseConstants.INPUT_SOURCE_NAME} as t1 WHERE t1.PrimaryAccountNumber == ""9999941616073663_1"" " +
                                   $@"WITH {DatabaseConstants.INPUT_SOURCE_NAME} as t2 WHERE t2.PrimaryAccountNumber == ""9999941616073663_2"" " +
                                   "ON t1.AcquiringInstitutionIdentificationCode == t2.AcquiringInstitutionIdentificationCode " +
                                   "TIMEOUT '00:00:02' " +
                                   $"SELECT (string)t1.PrimaryAccountNumber as c1, t2.PrimaryAccountNumber as c2, 1 as numeroXXX, 2 as numeroYYY into {sourceName}";

            string command = $"create source {sourceName} (c1 string(4000), c2 string(4000), numeroXXX int); create stream {oldStreamName} {{ {eql} }}; alter source {sourceName} add (numeroYYY int); alter stream {oldStreamName} with query = {{ {eql2} }}";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    kernel.Bind<ISourceTypeFactory>().ToConstructor(x => new SourceTypeFactory());
                    kernel.Bind<ISource>().ToConstructor(x => new ConcreteSource());
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        Stream stream = dbContext.Streams.Single(x => x.StreamName == oldStreamName);
                        Assert.AreEqual(oldStreamName, stream.StreamName);
                        Assert.IsTrue(stream.IsActive);
                        Assert.AreEqual(eql2.Replace('\n', '\0').Trim(), stream.Query);
                        StreamColumn[] projectionColumns = dbContext.StreamColumns.Where(x => x.ServerId == stream.ServerId
                                            && x.DatabaseId == stream.DatabaseId
                                            && x.SchemaId == stream.SchemaId
                                            && x.StreamId == stream.StreamId).ToArray();

                        Assert.IsTrue(projectionColumns.Any(x => x.ColumnName == "c1" && x.ColumnType == typeof(string).AssemblyQualifiedName));
                        Assert.IsTrue(projectionColumns.Any(x => x.ColumnName == "c2" && x.ColumnType == typeof(string).AssemblyQualifiedName));
                        Assert.IsTrue(projectionColumns.Any(x => x.ColumnName == "numeroXXX" && x.ColumnType == typeof(int).AssemblyQualifiedName));
                        Assert.IsTrue(projectionColumns.Any(x => x.ColumnName == "numeroYYY" && x.ColumnType == typeof(int).AssemblyQualifiedName));
                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el rol de base de datos '{newStreamName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Alters the query of a stream renaming columns of its projection, i. e. the select statement.
        /// </summary>
        [TestMethod]
        public void AlterStreamRenameProjectionColumns()
        {
            string oldStreamName = "oldStream";
            string newStreamName = "newStream";
            string sourceName = "newSource";
            string eql = "cross " +
                                   $@"JOIN {DatabaseConstants.INPUT_SOURCE_NAME} as t1 WHERE t1.PrimaryAccountNumber == ""9999941616073663_1"" " +
                                   $@"WITH {DatabaseConstants.INPUT_SOURCE_NAME} as t2 WHERE t2.PrimaryAccountNumber == ""9999941616073663_2"" " +
                                   "ON t1.AcquiringInstitutionIdentificationCode == t2.AcquiringInstitutionIdentificationCode " +
                                   "TIMEOUT '00:00:02' " +
                                   $"SELECT (string)t1.PrimaryAccountNumber as c1, t2.PrimaryAccountNumber as c2, 1 as numeroXXX into {sourceName}";

            string eql2 = "cross " +
                                   $@"JOIN {DatabaseConstants.INPUT_SOURCE_NAME} as t1 WHERE t1.PrimaryAccountNumber == ""9999941616073663_1"" " +
                                   $@"WITH {DatabaseConstants.INPUT_SOURCE_NAME} as t2 WHERE t2.PrimaryAccountNumber == ""9999941616073663_2"" " +
                                   "ON t1.AcquiringInstitutionIdentificationCode == t2.AcquiringInstitutionIdentificationCode " +
                                   "TIMEOUT '00:00:02' " +
                                   $"SELECT (string)t1.PrimaryAccountNumber as column1, t2.PrimaryAccountNumber as column2, 1 as column3 into {sourceName}";

            string command = $"create source {sourceName} (column1 string(4000), column2 string(4000), column3 int, c1 string(4000), c2 string(4000), numeroXXX int); create stream {oldStreamName} {{ {eql} }}; alter stream {oldStreamName} with query = {{ {eql2} }}";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    SpaceAssemblyBuilder sasmBuilder = new SpaceAssemblyBuilder("Test");
                    AssemblyBuilder asmBuilder = sasmBuilder.CreateAssemblyBuilder();
                    SpaceModuleBuilder smodBuilder = new SpaceModuleBuilder(asmBuilder);
                    smodBuilder.CreateModuleBuilder();
                    kernel.Bind<AssemblyBuilder>().ToConstant(asmBuilder);
                    kernel.Bind<ISourceTypeFactory>().ToConstructor(x => new SourceTypeFactory());
                    kernel.Bind<ISource>().ToConstructor(x => new ConcreteSource());
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        Stream stream = dbContext.Streams.Single(x => x.StreamName == oldStreamName);
                        Assert.AreEqual(oldStreamName, stream.StreamName);
                        Assert.IsTrue(stream.IsActive);
                        Assert.AreEqual(eql2.Replace('\n', '\0').Trim(), stream.Query);
                        StreamColumn[] projectionColumns = dbContext.StreamColumns.Where(x => x.ServerId == stream.ServerId
                                            && x.DatabaseId == stream.DatabaseId
                                            && x.SchemaId == stream.SchemaId
                                            && x.StreamId == stream.StreamId).ToArray();

                        Assert.IsFalse(projectionColumns.Any(x => x.ColumnName == "c1" && x.ColumnType == typeof(string).AssemblyQualifiedName));
                        Assert.IsFalse(projectionColumns.Any(x => x.ColumnName == "c2" && x.ColumnType == typeof(string).AssemblyQualifiedName));
                        Assert.IsFalse(projectionColumns.Any(x => x.ColumnName == "numeroXXX" && x.ColumnType == typeof(int).AssemblyQualifiedName));

                        Assert.IsTrue(projectionColumns.Any(x => x.ColumnName == "column1" && x.ColumnType == typeof(string).AssemblyQualifiedName));
                        Assert.IsTrue(projectionColumns.Any(x => x.ColumnName == "column2" && x.ColumnType == typeof(string).AssemblyQualifiedName));
                        Assert.IsTrue(projectionColumns.Any(x => x.ColumnName == "column3" && x.ColumnType == typeof(int).AssemblyQualifiedName));
                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el rol de base de datos '{newStreamName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Alters the query of a stream renaming a column of its projection, i. e. the select statement, making it incompatible with the output source column structure.
        /// </summary>
        [TestMethod]
        public void AlterStreamRenameProjectionColumnsSourceIncompatible()
        {
            string oldStreamName = "oldStream";
            string sourceName = "newSource";
            string eql = "cross " +
                                   $@"JOIN {DatabaseConstants.INPUT_SOURCE_NAME} as t1 WHERE t1.PrimaryAccountNumber == ""9999941616073663_1"" " +
                                   $@"WITH {DatabaseConstants.INPUT_SOURCE_NAME} as t2 WHERE t2.PrimaryAccountNumber == ""9999941616073663_2"" " +
                                   "ON t1.AcquiringInstitutionIdentificationCode == t2.AcquiringInstitutionIdentificationCode " +
                                   "TIMEOUT '00:00:02' " +
                                   /* "WHERE  t1.@event.Message.#1.#43 == \"Shell El RodeoGUATEMALA    GT\" " + */
                                   $"SELECT (string)t1.PrimaryAccountNumber as c1, t2.PrimaryAccountNumber as c2, 1 as numeroXXX into {sourceName}";

            string eql2 = "cross " +
                                   $@"JOIN {DatabaseConstants.INPUT_SOURCE_NAME} as t1 WHERE t1.PrimaryAccountNumber == ""9999941616073663_1"" " +
                                   $@"WITH {DatabaseConstants.INPUT_SOURCE_NAME} as t2 WHERE t2.PrimaryAccountNumber == ""9999941616073663_2"" " +
                                   "ON t1.AcquiringInstitutionIdentificationCode == t2.AcquiringInstitutionIdentificationCode " +
                                   "TIMEOUT '00:00:02' " +
                                   /* "WHERE  t1.@event.Message.#1.#43 == \"Shell El RodeoGUATEMALA    GT\" " + */
                                   $"SELECT (string)t1.PrimaryAccountNumber as column1, t2.PrimaryAccountNumber as column2, 1 as column3 into {sourceName}";

            string command = $"create source {sourceName} (column1 string(4000), column3 int, c1 string, c2 object, numeroXXX int); create stream {oldStreamName} {{ {eql} }}; alter stream {oldStreamName} with query = {{ {eql2} }}";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    SpaceAssemblyBuilder sasmBuilder = new SpaceAssemblyBuilder("Test");
                    AssemblyBuilder asmBuilder = sasmBuilder.CreateAssemblyBuilder();
                    SpaceModuleBuilder smodBuilder = new SpaceModuleBuilder(asmBuilder);
                    smodBuilder.CreateModuleBuilder();
                    kernel.Bind<AssemblyBuilder>().ToConstant(asmBuilder);
                    kernel.Bind<ISourceTypeFactory>().ToConstructor(x => new SourceTypeFactory());
                    kernel.Bind<ISource>().ToConstructor(x => new ConcreteSource());
                    try
                    {
                        FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                        tran.Rollback();
                        Assert.Fail("Un stream con proyección incompatible con la funente especificada en el into fue creado.");
                    }
                    catch (Exception)
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Alters the stream status to on.
        /// </summary>
        [TestMethod]
        public void AlterStreamWithStatusOn()
        {
            string streamName = "newStream";
            string sourceName = "newSource";
            string eql = "cross " +
                                   $@"JOIN {DatabaseConstants.INPUT_SOURCE_NAME} as t1 WHERE t1.PrimaryAccountNumber == ""9999941616073663_1"" " +
                                   $@"WITH {DatabaseConstants.INPUT_SOURCE_NAME} as t2 WHERE t2.PrimaryAccountNumber == ""9999941616073663_2"" " +
                                   "ON t1.AcquiringInstitutionIdentificationCode == t2.AcquiringInstitutionIdentificationCode " +
                                   "TIMEOUT '00:00:02' " +
                                   $"SELECT (string)t1.PrimaryAccountNumber as c1, t2.PrimaryAccountNumber as c2, 1 as numeroXXX into {sourceName} ";

            string command = $"create source {sourceName} (c1 string(4000), c2 string(4000), numeroXXX int); create stream {streamName} {{ {eql} }} with status = off; alter stream {streamName} with status = on";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    SpaceAssemblyBuilder sasmBuilder = new SpaceAssemblyBuilder("Test");
                    AssemblyBuilder asmBuilder = sasmBuilder.CreateAssemblyBuilder();
                    SpaceModuleBuilder smodBuilder = new SpaceModuleBuilder(asmBuilder);
                    smodBuilder.CreateModuleBuilder();
                    kernel.Bind<AssemblyBuilder>().ToConstant(asmBuilder);
                    kernel.Bind<ISourceTypeFactory>().ToConstructor(x => new SourceTypeFactory());
                    kernel.Bind<ISource>().ToConstructor(x => new ConcreteSource());
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        Stream stream = dbContext.Streams.Single(x => x.StreamName == streamName);
                        Assert.AreEqual(streamName, stream.StreamName);
                        Assert.IsTrue(stream.IsActive);
                        Assert.AreEqual(eql.Replace('\n', '\0').Trim(), stream.Query);
                        StreamColumn[] projectionColumns = dbContext.StreamColumns.Where(x => x.ServerId == stream.ServerId
                                            && x.DatabaseId == stream.DatabaseId
                                            && x.SchemaId == stream.SchemaId
                                            && x.StreamId == stream.StreamId).ToArray();

                        Assert.IsTrue(projectionColumns.Any(x => x.ColumnName == "c1" && x.ColumnType == typeof(string).AssemblyQualifiedName));
                        Assert.IsTrue(projectionColumns.Any(x => x.ColumnName == "c2" && x.ColumnType == typeof(string).AssemblyQualifiedName));
                        Assert.IsTrue(projectionColumns.Any(x => x.ColumnName == "numeroXXX" && x.ColumnType == typeof(int).AssemblyQualifiedName));
                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el rol de base de datos '{streamName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Alters the stream status to off.
        /// </summary>
        [TestMethod]
        public void AlterStreamWithStatusOff()
        {
            string streamName = "newStream";
            string sourceName = "newSource";
            string eql = "cross " +
                                   $@"JOIN {DatabaseConstants.INPUT_SOURCE_NAME} as t1 WHERE t1.PrimaryAccountNumber == ""9999941616073663_1"" " +
                                   $@"WITH {DatabaseConstants.INPUT_SOURCE_NAME} as t2 WHERE t2.PrimaryAccountNumber == ""9999941616073663_2"" " +
                                   "ON t1.AcquiringInstitutionIdentificationCode == t2.AcquiringInstitutionIdentificationCode " +
                                   "TIMEOUT '00:00:02' " +
                                   $"SELECT (string)t1.PrimaryAccountNumber as c1, t2.PrimaryAccountNumber as c2, 1 as numeroXXX into {sourceName} ";

            string command = $"create source {sourceName} (c1 string(4000), c2 string(4000), numeroXXX int); create stream {streamName} {{ {eql} }} with status = on; alter stream {streamName} with status = off";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    SpaceAssemblyBuilder sasmBuilder = new SpaceAssemblyBuilder("Test");
                    AssemblyBuilder asmBuilder = sasmBuilder.CreateAssemblyBuilder();
                    SpaceModuleBuilder smodBuilder = new SpaceModuleBuilder(asmBuilder);
                    smodBuilder.CreateModuleBuilder();
                    kernel.Bind<AssemblyBuilder>().ToConstant(asmBuilder);
                    kernel.Bind<ISourceTypeFactory>().ToConstructor(x => new SourceTypeFactory());
                    kernel.Bind<ISource>().ToConstructor(x => new ConcreteSource());
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        Stream stream = dbContext.Streams.Single(x => x.StreamName == streamName);
                        Assert.AreEqual(streamName, stream.StreamName);
                        Assert.IsFalse(stream.IsActive);
                        Assert.AreEqual(eql.Replace('\n', '\0').Trim(), stream.Query);
                        StreamColumn[] projectionColumns = dbContext.StreamColumns.Where(x => x.ServerId == stream.ServerId
                                            && x.DatabaseId == stream.DatabaseId
                                            && x.SchemaId == stream.SchemaId
                                            && x.StreamId == stream.StreamId).ToArray();

                        Assert.IsTrue(projectionColumns.Any(x => x.ColumnName == "c1" && x.ColumnType == typeof(string).AssemblyQualifiedName));
                        Assert.IsTrue(projectionColumns.Any(x => x.ColumnName == "c2" && x.ColumnType == typeof(string).AssemblyQualifiedName));
                        Assert.IsTrue(projectionColumns.Any(x => x.ColumnName == "numeroXXX" && x.ColumnType == typeof(int).AssemblyQualifiedName));
                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el rol de base de datos '{streamName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        #endregion alter stream

        #endregion alter

        #region drop

        /// <summary>
        /// Drops a login.
        /// </summary>
        [TestMethod]
        public void DropLogin()
        {
            string loginName = "login1";
            string password = "pass1234";
            string command = $"create login {loginName} with password = \"{password}\"; drop login {loginName}";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        bool exists = dbContext.Logins.Any(x => x.LoginName == loginName);
                        Assert.IsFalse(exists);
                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al eliminar el login '{loginName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Drops a database.
        /// </summary>
        [TestMethod]
        public void DropDatabase()
        {
            string databaseName = "newDatabase";
            string command = $"create database {databaseName}; drop database {databaseName}";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        bool exists = dbContext.Databases.Any(x => x.DatabaseName == databaseName);
                        Assert.IsFalse(exists);
                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al eliminar la base de datos '{databaseName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Drops a user.
        /// </summary>
        [TestMethod]
        public void DropUser()
        {
            string userName = "newUser";
            string loginUserName = DatabaseConstants.ADMIN_LOGIN_1_NAME;
            string command = $"use {DatabaseConstants.TEST_DATABASE_NAME}; create user {userName} with login = {loginUserName}; drop user {userName}";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        bool exists = dbContext.DatabaseUsers.Any(x => x.DbUsrName == userName);
                        Assert.IsFalse(exists);
                        tran.Rollback();
                    }
                    catch
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al eliminar el usuario '{userName}'.");
                    }
                }
            }
        }

        /// <summary>
        /// Drops a database role.
        /// </summary>
        [TestMethod]
        public void DropDatabaseRole()
        {
            string roleName = DatabaseConstants.INEXISTENT_ROLE_NAME_1;
            string command = $"create role {roleName}; drop role {roleName}";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        bool exists = dbContext.DatabaseRoles.Any(x => x.DbRoleName == roleName);
                        Assert.IsFalse(exists);
                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el rol de base de datos '{roleName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Drops a schema.
        /// </summary>
        [TestMethod]
        public void DropSchema()
        {
            string schemaName = "newSchema";
            string command = $"create schema {schemaName}; drop schema {schemaName}";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        bool exists = dbContext.Schemas.Any(x => x.SchemaName == schemaName);
                        Assert.IsFalse(exists);
                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el rol de base de datos '{schemaName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Drops a source.
        /// </summary>
        [TestMethod]
        public void DropSource()
        {
            string sourceName = "newSource";
            string command = $"create source {sourceName} (column1 int, column2 double, column3 string(4000)); drop source {sourceName}";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    SpaceAssemblyBuilder sasmBuilder = new SpaceAssemblyBuilder("Test");
                    AssemblyBuilder asmBuilder = sasmBuilder.CreateAssemblyBuilder();
                    SpaceModuleBuilder smodBuilder = new SpaceModuleBuilder(asmBuilder);
                    smodBuilder.CreateModuleBuilder();
                    kernel.Bind<AssemblyBuilder>().ToConstant(asmBuilder);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        bool exists = dbContext.Sources.Any(x => x.SourceName == sourceName);
                        Assert.IsFalse(exists);
                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el rol de base de datos '{sourceName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Drops a stream.
        /// </summary>
        [TestMethod]
        public void DropStream()
        {
            string sourceName = "sourceNameXX";
            string streamName = "newStream";
            string eql = "cross " +
                                   $@"JOIN {DatabaseConstants.INPUT_SOURCE_NAME} as t1 WHERE t1.PrimaryAccountNumber == ""9999941616073663_1"" " +
                                   $@"WITH {DatabaseConstants.INPUT_SOURCE_NAME} as t2 WHERE t2.PrimaryAccountNumber == ""9999941616073663_2"" " +
                                   "ON t1.AcquiringInstitutionIdentificationCode == t2.AcquiringInstitutionIdentificationCode " +
                                   "TIMEOUT '00:00:02' " +
                                   $"SELECT (string)t1.PrimaryAccountNumber as c1, t2.PrimaryAccountNumber as c2, 1 as numeroXXX into {sourceName} ";

            string command = $"create source {sourceName} (c1 string(4000), c2 string(4000), numeroXXX int); create stream {streamName} {{ {eql} }}; drop stream {streamName}";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    kernel.Bind<ISourceTypeFactory>().ToConstructor(x => new SourceTypeFactory());
                    kernel.Bind<ISource>().ToConstructor(x => new ConcreteSource());

                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        bool exists = dbContext.Streams.Any(x => x.StreamName == streamName);
                        Assert.IsFalse(exists);
                        bool existColumns = dbContext.StreamColumns.Any(x => x.Stream.StreamName == streamName);
                        Assert.IsFalse(existColumns);
                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el rol de base de datos '{streamName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        #endregion drop

        #region truncate

        /// <summary>
        /// Truncates a source deleting all messages "saved" (pending of review) in it.
        /// </summary>
        [TestMethod]
        public void TruncateSource()
        {
            string sourceName = "newSource";
            string command = $"create source {sourceName} (column1 int, column2 double, column3 string(4000)); truncate source {sourceName}";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    try
                    {
                        SpaceAssemblyBuilder sasmBuilder1 = new SpaceAssemblyBuilder("Test");
                        AssemblyBuilder asmBuilder1 = sasmBuilder1.CreateAssemblyBuilder();
                        SpaceModuleBuilder smodBuilder1 = new SpaceModuleBuilder(asmBuilder1);
                        smodBuilder1.CreateModuleBuilder();
                        kernel.Bind<AssemblyBuilder>().ToConstant(asmBuilder1);
                        FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                        bool exists = dbContext.Sources.Any(x => x.SourceName == sourceName);
                        Assert.IsTrue(exists);
                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el rol de base de datos '{sourceName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        #endregion truncate

        #region otros

        /// <summary>
        /// Grants permission test.
        /// </summary>
        [TestMethod]
        public void TestGrantPermissionOnDatabase()
        {
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;
            string principalName = DatabaseConstants.DBO_USER_NAME;
            string command = $"grant alter any user to user {principalName}";
            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
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

        /// <summary>
        /// Creates multiple schemas.
        /// </summary>
        [TestMethod]
        public void TestCreateSchemas()
        {
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;
            string newSchemaName = "Schema123";
            string command = $"create schema {newSchemaName}; create schema {newSchemaName + "XX"}; create schema {newSchemaName + "YY"}";
            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Server server = dbContext.Servers.Single(x => x.ServerName == DatabaseConstants.TEST_SERVER_NAME);
                    Space.Database.Database db = dbContext.Databases.Single(x => x.ServerId == server.ServerId && x.DatabaseName == DatabaseConstants.MASTER_DATABASE_NAME);
                    Space.Database.Schema schema1 = dbContext.Schemas.Single(x => x.ServerId == db.ServerId && x.DatabaseId == db.DatabaseId && x.SchemaName == newSchemaName);
                    Space.Database.Schema schema2 = dbContext.Schemas.Single(x => x.ServerId == db.ServerId && x.DatabaseId == db.DatabaseId && x.SchemaName == newSchemaName + "XX");
                    Space.Database.Schema schema3 = dbContext.Schemas.Single(x => x.ServerId == db.ServerId && x.DatabaseId == db.DatabaseId && x.SchemaName == newSchemaName + "YY");
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

        /// <summary>
        /// Creates a new stream with a non administrator login and user.
        /// </summary>
        [TestMethod]
        public void TestCreateStream()
        {
            string userNameThatCreateTheStream = DatabaseConstants.NORMAL_USER_2_NAME;
            string sourceNameTest = "source1234";
            string sourceNameTest2 = "sourceForInto";
            string databaseName = DatabaseConstants.MASTER_DATABASE_NAME;
            string firstCommand = $@"use {databaseName}; 
                                        create source {sourceNameTest2} (c1 string(4000), c3 string(4000)); 
                                        create source {sourceNameTest} (MessageType string(4000), RetrievalReferenceNumber string(4000), PrimaryAccountNumber string(4000), SourceTimestamp datetime, column1 int, column2 double, column3 string(4000)); 
                                        grant connect on database {databaseName}, create stream, read on source {sourceNameTest}, write on source {sourceNameTest2} to user {userNameThatCreateTheStream}";

            string newStreamName = "Stream1234";
            string eql = $@"cross 
                                  JOIN {sourceNameTest} as t1 WHERE t1.MessageType == ""0100""
                                  WITH {sourceNameTest} as t2 WHERE t2.MessageType == ""0110""
                                  ON (string)t1.PrimaryAccountNumber == (string)t2.PrimaryAccountNumber and (string)t1.RetrievalReferenceNumber == (string)t2.RetrievalReferenceNumber 
                                  TIMEOUT '00:00:01.5' 
                                  WHERE isnull(t2.SourceTimestamp, '01/01/2017') - isnull(t1.SourceTimestamp, '01/01/2016') <= '00:00:01'
                                  SELECT
                                          t1.PrimaryAccountNumber as c1,
                                          t2.PrimaryAccountNumber as c3 
                                  INTO {sourceNameTest2} ";

            string secondCommand = $"use {databaseName}; create stream {newStreamName} {{\n{eql}\n}}";
            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(firstCommand, kernel);

                    kernel = new StandardKernel();
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    kernel.Bind<ISourceTypeFactory>().ToConstructor(x => new SourceTypeFactory());
                    kernel.Bind<ISource>().ToConstructor(x => new ConcreteSource());
                    this.loginName = DatabaseConstants.NORMAL_LOGIN_2_NAME;
                    FirstLevelPipelineContext result2 = this.ProcessCommand(secondCommand, kernel);

                    Server server = dbContext.Servers.Single(x => x.ServerName == DatabaseConstants.TEST_SERVER_NAME);
                    Space.Database.Database db = dbContext.Databases.Single(x => x.ServerId == server.ServerId && x.DatabaseName == databaseName);
                    Space.Database.Schema schema = dbContext.Schemas.Single(x => x.ServerId == server.ServerId && x.DatabaseId == db.DatabaseId && x.SchemaName == DatabaseConstants.DBO_SCHEMA_NAME);
                    Space.Database.Stream stream = dbContext.Streams.Single(x => x.ServerId == schema.ServerId && x.DatabaseId == schema.DatabaseId && x.SchemaId == schema.SchemaId && x.StreamName == newStreamName);
                    Assert.IsNotNull(stream);
                    Assert.AreEqual<string>(newStreamName, stream.StreamName);

                    Assert.IsTrue(stream.ProjectionColumns.Any(x => x.ColumnName == "c1" && x.ColumnType == typeof(string).AssemblyQualifiedName));
                    Assert.IsTrue(stream.ProjectionColumns.Any(x => x.ColumnName == "c3" && x.ColumnType == typeof(string).AssemblyQualifiedName));

                    Console.WriteLine();
                    tran.Rollback();
                }
            }
        }

        /// <summary>
        /// Creates a new role.
        /// </summary>
        [TestMethod]
        public void TestCreateRole()
        {
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;
            string newRoleName = "Role123";
            string command = "create role " + newRoleName;
            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Server server = dbContext.Servers.Single(x => x.ServerName == DatabaseConstants.TEST_SERVER_NAME);
                    Space.Database.Database db = dbContext.Databases.Single(x => x.DatabaseName == DatabaseConstants.MASTER_DATABASE_NAME);
                    Space.Database.DatabaseRole role = dbContext.DatabaseRoles.Single(x => x.ServerId == server.ServerId && x.DatabaseId == db.DatabaseId && x.DbRoleName == newRoleName);
                    Assert.IsNotNull(role);
                    Assert.AreEqual<string>(newRoleName, role.DbRoleName);

                    Console.WriteLine();
                    tran.Rollback();
                }
            }
        }

        /// <summary>
        /// Creates a new database.
        /// </summary>
        [TestMethod]
        public void TestCreateDatabase()
        {
            string newDatabaseName = "Database1234";
            this.loginName = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = "create database " + newDatabaseName;
            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Server server = dbContext.Servers.Single(x => x.ServerName == DatabaseConstants.TEST_SERVER_NAME);
                    Space.Database.Database database = dbContext.Databases.Single(x => x.ServerId == server.ServerId && x.DatabaseName == newDatabaseName);
                    Assert.IsNotNull(database);
                    Assert.AreEqual<string>(newDatabaseName, database.DatabaseName);

                    Console.WriteLine();
                    tran.Rollback();
                }
            }
        }

        /// <summary>
        /// Creates a new login with a non administrator login and user.
        /// </summary>
        [TestMethod]
        public void TestCreateLogin()
        {
            string newloginName = "Login123";
            string loginNameThatCreateTheLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string firstCommand = $"grant alter any login to login {loginNameThatCreateTheLogin}";
            string secondCommand = $"create login {newloginName} with password = \"abc\", default_database = {DatabaseConstants.MASTER_DATABASE_NAME}";
            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(firstCommand, kernel);

                    this.loginName = loginNameThatCreateTheLogin;
                    FirstLevelPipelineContext result2 = this.ProcessCommand(secondCommand, kernel);
                    Server server = dbContext.Servers.Single(x => x.ServerName == DatabaseConstants.TEST_SERVER_NAME);
                    Space.Database.Database db = dbContext.Databases.Single(x => x.DatabaseName == DatabaseConstants.MASTER_DATABASE_NAME);
                    Space.Database.Login login = dbContext.Logins.Single(x => x.ServerId == server.ServerId && x.LoginName == newloginName);
                    Assert.IsNotNull(login);
                    Assert.AreEqual<string>(newloginName, login.LoginName);

                    Console.WriteLine();
                    tran.Rollback();
                }
            }
        }

        /// <summary>
        /// Creates a new source with a non administrator login and user.
        /// </summary>
        [TestMethod]
        public void TestCreateSource()
        {
            string newSource = "SourceInicial_nueva";
            string userNameThatCreateTheSource = DatabaseConstants.NORMAL_USER_1_NAME;
            string firstCommand = $"grant create source to user {userNameThatCreateTheSource}";
            string secondCommand = $"create source {newSource} (column1 int, column2 double, column3 string(4000))";
            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    SpaceAssemblyBuilder sasmBuilder = new SpaceAssemblyBuilder("Test");
                    AssemblyBuilder asmBuilder = sasmBuilder.CreateAssemblyBuilder();
                    SpaceModuleBuilder smodBuilder = new SpaceModuleBuilder(asmBuilder);
                    smodBuilder.CreateModuleBuilder();
                    kernel.Bind<AssemblyBuilder>().ToConstant(asmBuilder);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(firstCommand, kernel);

                    this.loginName = DatabaseConstants.NORMAL_LOGIN_1_NAME;
                    FirstLevelPipelineContext result2 = this.ProcessCommand(secondCommand, kernel);

                    Server server = dbContext.Servers.Single(x => x.ServerName == DatabaseConstants.TEST_SERVER_NAME);
                    Space.Database.Database db = dbContext.Databases.Single(x => x.DatabaseName == DatabaseConstants.MASTER_DATABASE_NAME);
                    Space.Database.Source source = dbContext.Sources.Single(x => x.ServerId == server.ServerId && x.DatabaseId == db.DatabaseId && x.SourceName == newSource);
                    Assert.IsNotNull(source);
                    Assert.AreEqual<string>(newSource, source.SourceName);

                    Console.WriteLine();
                    tran.Rollback();
                }
            }
        }

        #endregion otros

        /// <summary>
        /// This method create a pipeline context and execute the specified command.
        /// </summary>
        /// <param name="command">Command to execute.</param>
        /// <param name="kernel">DI kernel.</param>
        /// <returns>Pipeline context.</returns>
        private FirstLevelPipelineContext ProcessCommand(string command, IKernel kernel)
        {
            IBinding binding = kernel.GetBindings(typeof(Language.IGrammarRuleValidator)).FirstOrDefault();
            if (binding != null)
            {
                kernel.RemoveBinding(binding);
            }

            kernel.Bind<Language.IGrammarRuleValidator>().ToConstant(new TestRuleValidator());
            CommandPipelineBuilder cpb = new CommandPipelineBuilder();
            Filter<FirstLevelPipelineContext, FirstLevelPipelineContext> pipeline = cpb.Build();

            FirstLevelPipelineExecutor cpe = new FirstLevelPipelineExecutor(pipeline);
            FirstLevelPipelineContext context = new FirstLevelPipelineContext(command, this.loginName, kernel);
            FirstLevelPipelineContext result = cpe.Execute(context);
            return result;
        }

        /// <summary>
        /// Creates a code generator configuration object.
        /// </summary>
        /// <param name="dsf">A scheduler factory.</param>
        /// <param name="kernel">DI kernel.</param>
        /// <returns>Code generator configuration object.</returns>
        private CodeGeneratorConfiguration GetCodeGeneratorConfig(ManagementSchedulerFactory dsf, IKernel kernel)
        {
            bool printLog = false;
            bool debugMode = false;
            bool measureElapsedTime = false;
            bool isTestMode = false;
            Login login = new SpaceDbContext(initializer: null).Logins.Single(x => x.LoginName == DatabaseConstants.SA_LOGIN_NAME);
            SpaceAssemblyBuilder sasmBuilder = new SpaceAssemblyBuilder("Test");
            AssemblyBuilder asmBuilder = sasmBuilder.CreateAssemblyBuilder();
            SpaceModuleBuilder smodBuilder = new SpaceModuleBuilder(asmBuilder);
            smodBuilder.CreateModuleBuilder();
            kernel.Bind<ISourceTypeFactory>().ToConstructor(x => new SourceTypeFactory());
            kernel.Bind<ISource>().ToConstructor(x => new ConcreteSource());
            CodeGeneratorConfiguration config = new CodeGeneratorConfiguration(
                dsf,
                asmBuilder,
                kernel,
                printLog: printLog,
                debugMode: debugMode,
                measureElapsedTime: measureElapsedTime,
                isTestMode: isTestMode,
                queryName: "QueryTest");

            return config;
        }
    }
}
