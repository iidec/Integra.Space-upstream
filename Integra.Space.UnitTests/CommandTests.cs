﻿using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Integra.Space.Pipeline;
using Ninject;
using Integra.Space.Database;
using System.Data.Entity;
using Integra.Space.Compiler;
using System.Reflection.Emit;
using System.Threading.Tasks.Dataflow;
using System.Reflection;
using System.Reactive.Linq;

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

        private CodeGeneratorConfiguration GetCodeGeneratorConfig(ManagementSchedulerFactory dsf, IKernel kernel)
        {
            bool printLog = false;
            bool debugMode = false;
            bool measureElapsedTime = false;
            bool isTestMode = false;
            Login login = new SpaceDbContext().Logins.Single(x => x.LoginName == "sa");
            SpaceAssemblyBuilder sasmBuilder = new SpaceAssemblyBuilder("Test");
            AssemblyBuilder asmBuilder = sasmBuilder.CreateAssemblyBuilder();
            SpaceModuleBuilder smodBuilder = new SpaceModuleBuilder(asmBuilder);
            smodBuilder.CreateModuleBuilder();
            kernel.Bind<ISourceTypeFactory>().ToConstructor(x => new SourceTypeFactory());
            kernel.Bind<ISource>().ToConstructor(x => new ConcreteSource());
            CodeGeneratorConfiguration config = new CodeGeneratorConfiguration(
                login,
                dsf,
                asmBuilder,
                kernel,
                printLog: printLog,
                debugMode: debugMode,
                measureElapsedTime: measureElapsedTime,
                isTestMode: isTestMode,
                queryName: "QueryTest"
                );

            return config;
        }

        #region insert

        [TestMethod]
        public void Insert1()
        {
            #region commmand

            string command = @"insert into SourceParaPruebas
                                (
                                MessageType,
                                PrimaryAccountNumber,
                                ProcessingCode,
                                TransactionAmount,
                                DateTimeTransmission,
                                SystemTraceAuditNumber,
                                LocalTransactionTime,
                                LocalTransactionDate,
                                SettlementDate,
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
            
            loginName = "sa";
            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    #region create stream

                    string eql = "use Database2; from SourceParaPruebas where MessageType == \"0100\" select true as resultado into SourceParaPruebas";
                    ManagementSchedulerFactory dsf = new ManagementSchedulerFactory();
                    CodeGeneratorConfiguration context = this.GetCodeGeneratorConfig(dsf, kernel);
                    Language.CommandParser parser = new Language.CommandParser(eql);
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

                    IObservable<TestObject1> o = Observable.Create<TestObject1>(x => {
                        return BufferBlockForTest.BufferBlock1.AsObservable().Subscribe(y => x.OnNext((TestObject1)y));
                    });

                    IObservable<object> resultObservable = ((IObservable<object>)result.Invoke(queryObject, new object[] { o /*, dsf.TestScheduler*/ }));

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

        [TestMethod]
        public void GetMetadataFromWhereSelectOrderByTest()
        {
            string command = "use Database2; from sys.streams as x where (string)ServerId == \"59e858fc-c84d-48a7-8a98-c0e7adede20a\" apply window of '00:00:01' select ServerId as serverId, max(1) as entero order by desc serverId, entero into SourceParaMetadata";
            loginName = "sa";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        [TestMethod]
        public void GetMetadataFromSelectTest()
        {
            string command = "use Database2; from sys.streams as x select ServerId as serverId, 2 as entero into SourceParaMetadata";

            loginName = "sa";
            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext())
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
                    Database.Database database = dbContext.Databases.Single(x => x.ServerId == login.ServerId && x.DatabaseName == entityName);
                    Assert.AreEqual<string>("AdminLogin", database.Login.LoginName);

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
                    Database.Endpoint endpoint = dbContext.Endpoints.Single(x => x.ServerId == login.ServerId && x.EnpointName == entityName);
                    Assert.AreEqual<string>("AdminLogin", endpoint.Login.LoginName);

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
                    Database.Schema schema = dbContext.Schemas.Single(x => x.ServerId == login.ServerId && x.DatabaseId == database.DatabaseId && x.SchemaName == entityName);
                    Assert.AreEqual<string>("AdminUser", schema.DatabaseUser.DbUsrName);

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
                    Database.Source source = dbContext.Sources.Single(x => x.ServerId == login.ServerId && x.DatabaseId == database.DatabaseId && x.SchemaId == schema.SchemaId && x.SourceName == entityName);
                    Assert.AreEqual<string>("AdminUser", source.DatabaseUser.DbUsrName);

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
                    Database.Stream stream = dbContext.Streams.Single(x => x.ServerId == login.ServerId && x.DatabaseId == database.DatabaseId && x.SchemaId == schema.SchemaId && x.StreamName == entityName);
                    Assert.AreEqual<string>("AdminUser", stream.DatabaseUser.DbUsrName);

                    Console.WriteLine();
                    tran.Rollback();
                }
            }
        }

        #endregion take ownership

        #region add

        [TestMethod]
        public void AddUserToRole()
        {
            string roleName = "RoleForTest";
            string userName = "UserForTest";
            loginName = "AdminLogin";
            string command = $"add {userName} to {roleName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        Database.DatabaseUser dbUser = dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName).DatabaseUsers.Single(x => x.DbUsrName == userName);
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

        [TestMethod]
        public void AddUserToRoles()
        {
            string roleName1 = "RoleForTest";
            string roleName2 = "RoleForTest2";
            string userName = "UserForTest";
            string command = $"add {userName} to {roleName1}, {roleName2}";
            loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        Database.DatabaseUser dbUser1 = dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName1).DatabaseUsers.Single(x => x.DbUsrName == userName);
                        Assert.AreEqual(userName, dbUser1.DbUsrName);
                        Database.DatabaseUser dbUser2 = dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName2).DatabaseUsers.Single(x => x.DbUsrName == userName);
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

        [TestMethod]
        public void AddUserListToRole()
        {
            string roleName = "RoleForTest";
            string userName1 = "UserForTest";
            string userName2 = "UserAux";
            string command = $"add {userName1}, {userName2} to {roleName}";
            loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        Database.DatabaseUser dbUser1 = dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName).DatabaseUsers.Single(x => x.DbUsrName == userName1);
                        Assert.AreEqual(userName1, dbUser1.DbUsrName);
                        Database.DatabaseUser dbUser2 = dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName).DatabaseUsers.Single(x => x.DbUsrName == userName2);
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

        [TestMethod]
        public void AddUserListToRoles()
        {
            string roleName1 = "RoleForTest";
            string roleName2 = "RoleForTest2";
            string userName1 = "UserForTest";
            string userName2 = "UserAux";
            string command = $"add {userName1}, {userName2} to {roleName1}, {roleName2}";
            loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        Database.DatabaseUser dbUser1 = dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName1).DatabaseUsers.Single(x => x.DbUsrName == userName1);
                        Assert.AreEqual(userName1, dbUser1.DbUsrName);
                        Database.DatabaseUser dbUser2 = dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName2).DatabaseUsers.Single(x => x.DbUsrName == userName1);
                        Assert.AreEqual(userName1, dbUser2.DbUsrName);

                        Database.DatabaseUser dbUser3 = dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName1).DatabaseUsers.Single(x => x.DbUsrName == userName2);
                        Assert.AreEqual(userName2, dbUser3.DbUsrName);
                        Database.DatabaseUser dbUser4 = dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName2).DatabaseUsers.Single(x => x.DbUsrName == userName2);
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

        [TestMethod]
        public void RemoveUserToRole()
        {
            string roleName = "RoleForTest";
            string userName = "UserForTest";
            loginName = "AdminLogin";
            string command = $"add {userName} to {roleName}";
            command += $"; remove {userName} to {roleName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        [TestMethod]
        public void RemoveUserToRoles()
        {
            string roleName1 = "RoleForTest";
            string roleName2 = "RoleForTest2";
            string userName = "UserForTest";
            string command = $"add {userName} to {roleName1}, {roleName2}";
            command += $"; remove {userName} to {roleName1}, {roleName2}";
            loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        [TestMethod]
        public void RemoveUserListToRole()
        {
            string roleName = "RoleForTest";
            string userName1 = "UserForTest";
            string userName2 = "UserAux";
            string command = $"add {userName1}, {userName2} to {roleName}";
            command += $"; remove {userName1}, {userName2} to {roleName}";
            loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        [TestMethod]
        public void RemoveUserListToRoles()
        {
            string roleName1 = "RoleForTest";
            string roleName2 = "RoleForTest2";
            string userName1 = "UserForTest";
            string userName2 = "UserAux";
            string command = $"add {userName1}, {userName2} to {roleName1}, {roleName2}";
            command += $"; remove {userName1}, {userName2} to {roleName1}, {roleName2}";
            loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        [TestMethod]
        public void CreateLogin()
        {
            string loginName = "login1";
            string password = "pass1234";
            string command = $"create login {loginName} with password = \"{password}\"";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        [TestMethod]
        public void CreateLoginWithStatusOn()
        {
            string loginName = "login1";
            string password = "pass1234";
            string command = $"create login {loginName} with password = \"{password}\", status = on";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        [TestMethod]
        public void CreateLoginWithStatusOff()
        {
            string loginName = "login1";
            string password = "pass1234";
            string command = $"create login {loginName} with password = \"{password}\", status = off";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        [TestMethod]
        public void CreateLoginWithDefaultDatabase()
        {
            string loginName = "login1";
            string password = "pass1234";
            string databaseName = "Database1";
            string command = $"create login {loginName} with password = \"{password}\", default_database = {databaseName}";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        [TestMethod]
        public void CreateLoginWithDefaultDatabaseAndStatusOn()
        {
            string loginName = "login1";
            string password = "pass1234";
            string databaseName = "Database1";
            string command = $"create login {loginName} with password = \"{password}\", default_database = {databaseName}, status = on";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        [TestMethod]
        public void CreateLoginWithDefaultDatabaseAndStatusOff()
        {
            string loginName = "login1";
            string password = "pass1234";
            string databaseName = "Database1";
            string command = $"create login {loginName} with password = \"{password}\", default_database = {databaseName}, status = off";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        [TestMethod]
        public void CreateDatabase()
        {
            string databaseName = "newDatabase";
            string command = $"create database {databaseName}";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == databaseName);
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

        [TestMethod]
        public void CreateDatabaseWithStatusOn()
        {
            string databaseName = "newDatabase";
            string command = $"create database {databaseName} with status = on";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == databaseName);
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

        [TestMethod]
        public void CreateDatabaseWithStatusOff()
        {
            string databaseName = "newDatabase";
            string command = $"create database {databaseName} with status = off";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == databaseName);
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

        [TestMethod]
        public void CreateUserWithLogin()
        {
            string userName = "newUser";
            string loginUserName = "AdminLogin2";
            string command = $"create user {userName} with login = {loginUserName}";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        [TestMethod]
        public void CreateUserWithLoginDefaultSchema()
        {
            string userName = "newUser";
            string loginUserName = "AdminLogin2";
            string schemaName = "Schema1";
            string command = $"create user {userName} with login = {loginUserName}, default_schema = {schemaName}";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        [TestMethod]
        public void CreateUserWithStatusOn()
        {
            string userName = "newUser";
            string loginUserName = "AdminLogin2";
            string command = $"create user {userName} with login = {loginUserName}, status = on";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        [TestMethod]
        public void CreateUserWithStatusOff()
        {
            string userName = "newUser";
            string loginUserName = "AdminLogin2";
            string command = $"create user {userName} with login = {loginUserName}, status = off";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        [TestMethod]
        public void CreateUserWithDefaultSchemaLoginStatusOff()
        {
            string userName = "newUser";
            string loginUserName = "AdminLogin2";
            string schemaName = "Schema1";
            string command = $"create user {userName} with login = {loginUserName}, default_schema = {schemaName}, status = off";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        [TestMethod]
        public void CreateUserWithDefaultSchemaLoginStatusOn()
        {
            string userName = "newUser";
            string loginUserName = "AdminLogin2";
            string schemaName = "Schema1";
            string command = $"create user {userName} with login = {loginUserName}, default_schema = {schemaName}, status = on";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        [TestMethod]
        public void CreateRole()
        {
            string roleName = "role1";
            string command = $"create role {roleName}";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        [TestMethod]
        public void CreateRoleWithStatusOn()
        {
            string roleName = "role1";
            string command = $"create role {roleName} with status = on";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        [TestMethod]
        public void CreateRoleWithStatusOff()
        {
            string roleName = "role1";
            string command = $"create role {roleName} with status = off";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        DatabaseRole role = dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName);
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

        [TestMethod]
        public void CreateRoleAddUser()
        {
            string roleName = "role1";
            string userName = "UserAux";
            string command = $"Create role {roleName} with add = {userName}";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        [TestMethod]
        public void CreateRoleAddUsers()
        {
            string roleName = "role1";
            string userName1 = "UserAux";
            string userName2 = "UserForTest";
            string userName3 = "AdminUser";
            string command = $"Create role {roleName} with add = {userName1} {userName2} {userName3}";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        [TestMethod]
        public void CreateSchema()
        {
            string schemaName = "newSchema";
            string command = $"create schema {schemaName}";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        [TestMethod]
        public void CreateSource()
        {
            string sourceName = "newSource";
            string command = $"create source {sourceName} (column1 int, column2 decimal, column3 string)";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();

            SpaceAssemblyBuilder sasmBuilder = new SpaceAssemblyBuilder("Integra.Space.Sources");
            AssemblyBuilder asmBuilder = sasmBuilder.CreateAssemblyBuilder();
            SpaceModuleBuilder modBuilder = new SpaceModuleBuilder(asmBuilder);
            modBuilder.CreateModuleBuilder();
            kernel.Bind<AssemblyBuilder>().ToConstant(asmBuilder);

            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        [TestMethod]
        public void CreateSourceWithStatusOn()
        {
            string sourceName = "newSource";
            string command = $"create source {sourceName} (column1 int, column2 decimal, column3 string) with status = on";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        [TestMethod]
        public void CreateSourceWithStatusOff()
        {
            string sourceName = "newSource";
            string command = $"create source {sourceName} (column1 int, column2 decimal, column3 string) with status = off";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        [TestMethod]
        public void CreateSourceWithCacheSize()
        {
            string sourceName = "newSource";
            string command = $"create source {sourceName} (column1 int, column2 decimal, column3 string) with cache_size = 200";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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
                        Assert.AreEqual<uint>(200, source.CacheSize);
                        Assert.AreEqual<uint>(60, source.CacheDurability);
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

        [TestMethod]
        public void CreateSourceWithCacheDurability()
        {
            string sourceName = "newSource";
            string command = $"create source {sourceName} (column1 int, column2 decimal, column3 string) with cache_durability = 70";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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
                        Assert.AreEqual<uint>(100, source.CacheSize);
                        Assert.AreEqual<uint>(70, source.CacheDurability);
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

        [TestMethod]
        public void CreateSourceWithCacheDurabilityCacheSize()
        {
            string sourceName = "newSource";
            string command = $"create source {sourceName} (column1 int, column2 decimal, column3 string) with cache_durability = 70, cache_size = 200";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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
                        Assert.AreEqual<uint>(200, source.CacheSize);
                        Assert.AreEqual<uint>(70, source.CacheDurability);
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

        [TestMethod]
        public void CreateStream()
        {
            string streamName = "newStream";
            string sourceName = "newSource";
            string eql = "cross " +
                                   "JOIN SourceParaPruebas as t1 WHERE t1.PrimaryAccountNumber == \"9999941616073663_1\" " +
                                   "WITH SourceParaPruebas as t2 WHERE t2.PrimaryAccountNumber == \"9999941616073663_2\" " +
                                   "ON t1.AcquiringInstitutionIdentificationCode == t2.AcquiringInstitutionIdentificationCode " +
                                   "TIMEOUT '00:00:02' " +
                                   //"WHERE  t1.@event.Message.#1.#43 == \"Shell El RodeoGUATEMALA    GT\" " +
                                   $"SELECT (string)t1.PrimaryAccountNumber as c1, t2.PrimaryAccountNumber as c2, 1 as numeroXXX into {sourceName} ";

            string command = $"use Database2; create source {sourceName} (c1 string, c2 string, numeroXXX int); create stream {streamName} {{ {eql} }}";
            this.loginName = "sa";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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
                                   //"WHERE  t1.@event.Message.#1.#43 == \"Shell El RodeoGUATEMALA    GT\" " +
                                   $"SELECT (string)t1.PrimaryAccountNumber as c1, t2.PrimaryAccountNumber as c2, 1 as numeroXXX into {sourceName} ";

            string command = $"create source {sourceName} (c1 string, c2 object); create stream {streamName} {{ {eql} }}";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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
                    catch (Exception e)
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void CreateStreamWithStatusOn()
        {
            string streamName = "newStream";
            string sourceName = "newSource";
            string eql = "cross " +
                                   "JOIN SourceParaPruebas as t1 WHERE t1.PrimaryAccountNumber == \"9999941616073663_1\" " +
                                   "WITH SourceParaPruebas as t2 WHERE t2.PrimaryAccountNumber == \"9999941616073663_2\" " +
                                   "ON t1.AcquiringInstitutionIdentificationCode == t2.AcquiringInstitutionIdentificationCode " +
                                   "TIMEOUT '00:00:02' " +
                                   //"WHERE  t1.@event.Message.#1.#43 == \"Shell El RodeoGUATEMALA    GT\" " +
                                   $"SELECT (string)t1.PrimaryAccountNumber as c1, t2.PrimaryAccountNumber as c2, 1 as numeroXXX into {sourceName} ";

            string command = $"use Database2; create source {sourceName} (c1 string, c2 string, numeroXXX int); create stream {streamName} {{ {eql} }} with status = on";
            this.loginName = "sa";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        [TestMethod]
        public void CreateStreamWithStatusOff()
        {
            string streamName = "newStream";
            string sourceName = "newSource";
            string eql = "cross " +
                                   "JOIN SourceParaPruebas as t1 WHERE t1.PrimaryAccountNumber == \"9999941616073663_1\" " +
                                   "WITH SourceParaPruebas as t2 WHERE t2.PrimaryAccountNumber == \"9999941616073663_2\" " +
                                   "ON t1.AcquiringInstitutionIdentificationCode == t2.AcquiringInstitutionIdentificationCode " +
                                   "TIMEOUT '00:00:02' " +
                                   //"WHERE  t1.@event.Message.#1.#43 == \"Shell El RodeoGUATEMALA    GT\" " +
                                   $"SELECT (string)t1.PrimaryAccountNumber as c1, t2.PrimaryAccountNumber as c2, 1 as numeroXXX into {sourceName} ";

            string command = $"use Database2; create source {sourceName} (c1 string, c2 string, numeroXXX int); create stream {streamName} {{ {eql} }} with status = off";
            this.loginName = "sa";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        #endregion create stream

        #endregion create

        #region alter

        #region alter login

        [TestMethod]
        public void AlterLoginName()
        {
            string loginName = "login1";
            string newName = "newLogin";
            string password = "oldPassword";
            string command = $"create login {loginName} with password = \"{password}\"; alter login {loginName} with name = {newName}";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        [TestMethod]
        public void AlterLoginPassword()
        {
            string loginName = "login1";
            string oldPassword = "oldPassword";
            string newPassword = "newPassword";
            string command = $"create login {loginName} with password = \"{oldPassword}\"; alter login {loginName} with password = \"{newPassword}\"";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        [TestMethod]
        public void AlterLoginWithStatusOn()
        {
            string loginName = "login1";
            string password = "pass1234";
            string command = $"create login {loginName} with password = \"{password}\", status = off; alter login {loginName} with status = on";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        [TestMethod]
        public void AlterLoginWithStatusOff()
        {
            string loginName = "login1";
            string password = "pass1234";
            string command = $"create login {loginName} with password = \"{password}\", status = on; alter login {loginName} with status = off";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        [TestMethod]
        public void AlterLoginWithDefaultDatabase()
        {
            string loginName = "login1";
            string password = "pass1234";
            string oldDatabaseName = "Database1";
            string newDatabaseName = "Database2";
            string command = $"create login {loginName} with password = \"{password}\", default_database = {oldDatabaseName}; alter login {loginName} with default_database = {newDatabaseName}";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        [TestMethod]
        public void AlterLoginWithPasswordDefaultDatabaseAndStatusOn()
        {
            string loginName = "login1";
            string oldPassword = "pass";
            string newPassword = "pass1234";
            string oldDatabaseName = "Database1";
            string newDatabaseName = "Database2";
            string command = $"create login {loginName} with password = \"{oldPassword}\", default_database = {oldDatabaseName}, status = off; alter login {loginName} with password = \"{newPassword}\", default_database = {newDatabaseName}, status = on";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        [TestMethod]
        public void AlterLoginWithPasswordDefaultDatabaseAndStatusOff()
        {
            string loginName = "login1";
            string oldPassword = "pass";
            string newPassword = "pass1234";
            string oldDatabaseName = "Database1";
            string newDatabaseName = "Database2";
            string command = $"create login {loginName} with password = \"{oldPassword}\", default_database = {oldDatabaseName}, status = on; alter login {loginName} with password = \"{newPassword}\", default_database = {newDatabaseName}, status = off";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        [TestMethod]
        public void AlterDatabaseName()
        {
            string oldDatabaseName = "oldDatabase";
            string newDatabaseName = "newDatabase";
            string command = $"create database {oldDatabaseName}; alter database {oldDatabaseName} with name = {newDatabaseName}";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == newDatabaseName);
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

        [TestMethod]
        public void AlterDatabaseWithStatusOn()
        {
            string databaseName = "newDatabase";
            string command = $"create database {databaseName} with status = off; alter database {databaseName} with status = on";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == databaseName);
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

        [TestMethod]
        public void AlterDatabaseWithStatusOff()
        {
            string databaseName = "newDatabase";
            string command = $"create database {databaseName} with status = on; alter database {databaseName} with status = off";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == databaseName);
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

        [TestMethod]
        public void AlterUserWithName()
        {
            string oldUserName = "oldUser";
            string newUserName = "newUser";
            string loginUserName = "AdminLogin2";
            string command = $"create user {oldUserName} with login = {loginUserName}; alter user {oldUserName} with name = {newUserName}";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        [TestMethod]
        public void AlterUserWithLogin()
        {
            string userName = "newUser";
            string oldLoginName = "AdminLogin2";
            string newLoginName = "AdminLogin3";
            string command = $"create user {userName} with login = {oldLoginName}; alter user {userName} with login = {newLoginName}";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        [TestMethod]
        public void AlterUserWithLoginDefaultSchema()
        {
            string userName = "newUser";
            string oldLoginName = "AdminLogin2";
            string newLoginName = "AdminLogin3";
            string oldSchemaName = "Schema1";
            string newSchemaName = "Schema2";
            string command = $"create user {userName} with login = {oldLoginName}, default_schema = {oldSchemaName}; alter user {userName} with login = {newLoginName}, default_schema = {newSchemaName}";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        [TestMethod]
        public void AlterUserWithStatusOn()
        {
            string userName = "newUser";
            string loginUserName = "AdminLogin2";
            string command = $"create user {userName} with login = {loginUserName}, status = off; alter user {userName} with status = on";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        [TestMethod]
        public void AlterUserWithStatusOff()
        {
            string userName = "newUser";
            string loginUserName = "AdminLogin2";
            string command = $"create user {userName} with login = {loginUserName}, status = on; alter user {userName} with status = off";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        [TestMethod]
        public void AlterUserWithLoginDefaultSchemaLoginStatusOff()
        {
            string userName = "newUser";
            string oldLoginName = "AdminLogin2";
            string newLoginName = "AdminLogin3";
            string oldSchemaName = "Schema1";
            string newSchemaName = "Schema2";
            string command = $"create user {userName} with login = {oldLoginName}, default_schema = {oldSchemaName}, status = on; alter user {userName} with login = {newLoginName}, default_schema = {newSchemaName}, status = off";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        [TestMethod]
        public void AlterUserWithDefaultSchemaLoginStatusOn()
        {
            string userName = "newUser";
            string oldLoginName = "AdminLogin2";
            string newLoginName = "AdminLogin3";
            string oldSchemaName = "Schema1";
            string newSchemaName = "Schema2";
            string command = $"create user {userName} with login = {oldLoginName}, default_schema = {oldSchemaName}, status = off; alter user {userName} with login = {newLoginName}, default_schema = {newSchemaName}, status = on";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        [TestMethod]
        public void AlterRoleName()
        {
            string oldRoleName = "role1";
            string newRoleName = "role2";
            string command = $"create role {oldRoleName}; alter role {oldRoleName} with name = {newRoleName}";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        DatabaseRole role = dbContext.DatabaseRoles.Single(x => x.DbRoleName == newRoleName);
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

        [TestMethod]
        public void AlterRoleWithStatusOn()
        {
            string roleName = "role1";
            string command = $"create role {roleName} with status = off; alter role {roleName} with status = on";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        [TestMethod]
        public void AlterRoleWithStatusOff()
        {
            string roleName = "role1";
            string command = $"create role {roleName} with status = on; alter role {roleName} with status = off";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    try
                    {
                        DatabaseRole role = dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName);
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

        [TestMethod]
        public void AlterRoleAddUser()
        {
            string roleName = "role1";
            string userName = "UserAux";
            string command = $"create role {roleName}; alter role {roleName} with add = {userName}";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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
                        Assert.Fail($"Error al editar el rol de base de datos '{roleName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        [TestMethod]
        public void AlterRoleAddUsers()
        {
            string roleName = "role1";
            string userName1 = "UserAux";
            string userName2 = "UserForTest";
            string userName3 = "AdminUser";
            string command = $"Create role {roleName}; alter role {roleName} with add = {userName1} {userName2} {userName3}";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        [TestMethod]
        public void AlterRoleRemoveUser()
        {
            string roleName = "role1";
            string userName = "UserAux";
            string command = $"create role {roleName}; alter role {roleName} with add = {userName}; alter role {roleName} with remove = {userName}";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        [TestMethod]
        public void AlterRoleRemoveUsers()
        {
            string roleName = "role1";
            string userName1 = "UserAux";
            string userName2 = "UserForTest";
            string userName3 = "AdminUser";
            string command = $"Create role {roleName}; alter role {roleName} with add = {userName1} {userName2} {userName3}; alter role {roleName} with remove = {userName1} {userName2} {userName3}";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        [TestMethod]
        public void CreateSchemaWithName()
        {
            string oldSchemaName = "oldSchema";
            string newSchemaName = "newSchema";
            string command = $"create schema {oldSchemaName}; alter schema {oldSchemaName} with name = {newSchemaName}";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        [TestMethod]
        public void AlterSourceWithName()
        {
            string oldSourceName = "oldSourceName";
            string newSourceName = "newSource";
            string command = $"create source {oldSourceName} (column1 string, column2 int, column3 decimal); alter source {oldSourceName} with name = {newSourceName}";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        [TestMethod]
        public void AlterSourceAddColumns()
        {
            string sourceName = "oldSourceName";
            string command = $@"create source {sourceName} (column1 string, column2 int, column3 decimal); 
                                alter source {sourceName} add (columnX string, columnY int, columnZ decimal)";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        [TestMethod]
        public void AlterSourceRemoveColumns()
        {
            string sourceName = "oldSourceName";
            string command = $@"create source {sourceName} (column1 string, column2 int, column3 decimal, columnX string, columnY int, columnZ decimal); 
                                alter source {sourceName} remove (columnX string, columnY int, columnZ decimal)";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        [TestMethod]
        public void AlterSourceRemoveAllColumns()
        {
            string sourceName = "oldSourceName";
            string command = $@"create source {sourceName} (columnX string, columnY int, columnZ decimal); 
                                alter source {sourceName} remove columnX string, columnY int, columnZ decimal";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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
                    catch (Exception e)
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void AlterSourceWithStatusOn()
        {
            string sourceName = "newSource";
            string command = $"create source {sourceName} (column1 string, column2 int, column3 decimal) with status = off; alter source {sourceName} with status = on";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        [TestMethod]
        public void AlterSourceWithStatusOff()
        {
            string sourceName = "newSource";
            string command = $"create source {sourceName} (column1 string, column2 int, column3 decimal) with status = on; alter source {sourceName} with status = off";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        [TestMethod]
        public void AlterSourceWithPersistentOn()
        {
            string sourceName = "newSource";
            string command = $"create source {sourceName} (column1 string, column2 int, column3 decimal) with persistent = off; alter source {sourceName} with persistent = on";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        [TestMethod]
        public void AlterSourceWithPersistentOff()
        {
            string sourceName = "newSource";
            string command = $"create source {sourceName} (column1 string, column2 int, column3 decimal) with persistent = on; alter source {sourceName} with persistent = off";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        [TestMethod]
        public void AlterSourceWithCacheSize()
        {
            string sourceName = "newSource";
            string command = $"create source {sourceName} (column1 string, column2 int, column3 decimal) with status = on, cache_size = 100; alter source {sourceName} with status = off, cache_size = 200";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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
                        Assert.AreEqual<uint>(source.CacheSize, 200);
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

        [TestMethod]
        public void AlterSourceWithCacheDurability()
        {
            string sourceName = "newSource";
            string command = $"create source {sourceName} (column1 string, column2 int, column3 decimal) with status = on, cache_durability = 100; alter source {sourceName} with status = off, cache_durability = 70";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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
                        Assert.AreEqual<uint>(source.CacheDurability, 70);
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

        [TestMethod]
        public void AlterSourceWithCacheSizeCacheDurability()
        {
            string sourceName = "newSource";
            string command = $"create source {sourceName} (column1 string, column2 int, column3 decimal) with status = on, cache_size = 100, cache_durability = 100; alter source {sourceName} with status = off, cache_size = 200, cache_durability = 70";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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
                        Assert.AreEqual<uint>(source.CacheSize, 200);
                        Assert.AreEqual<uint>(source.CacheDurability, 70);
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

        [TestMethod]
        public void AlterStreamWithName()
        {
            string oldStreamName = "oldStream";
            string newStreamName = "newStream";
            string sourceName = "newSource";
            string eql = "cross " +
                                   "JOIN SourceParaPruebas as t1 WHERE t1.PrimaryAccountNumber == \"9999941616073663_1\" " +
                                   "WITH SourceParaPruebas as t2 WHERE t2.PrimaryAccountNumber == \"9999941616073663_2\" " +
                                   "ON t1.AcquiringInstitutionIdentificationCode == t2.AcquiringInstitutionIdentificationCode " +
                                   "TIMEOUT '00:00:02' " +
                                   //"WHERE  t1.@event.Message.#1.#43 == \"Shell El RodeoGUATEMALA    GT\" " +
                                   $"SELECT (string)t1.PrimaryAccountNumber as c1, t2.PrimaryAccountNumber as c2, 1 as numeroXXX into {sourceName}";

            string command = $"use Database2; create source {sourceName} (c1 string, c2 string, numeroXXX int); create stream {oldStreamName} {{ {eql} }}; alter stream {oldStreamName} with name = {newStreamName}";
            this.loginName = "sa";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        [TestMethod]
        public void AlterStreamRemoveProjectionColumns()
        {
            string oldStreamName = "oldStream";
            string newStreamName = "newStream";
            string sourceName = "newSource";
            string eql = "cross " +
                                   "JOIN SourceParaPruebas as t1 WHERE t1.PrimaryAccountNumber == \"9999941616073663_1\" " +
                                   "WITH SourceParaPruebas as t2 WHERE t2.PrimaryAccountNumber == \"9999941616073663_2\" " +
                                   "ON t1.AcquiringInstitutionIdentificationCode == t2.AcquiringInstitutionIdentificationCode " +
                                   "TIMEOUT '00:00:02' " +
                                   //"WHERE  t1.@event.Message.#1.#43 == \"Shell El RodeoGUATEMALA    GT\" " +
                                   $"SELECT (string)t1.PrimaryAccountNumber as c1, t2.PrimaryAccountNumber as c2, 1 as numeroXXX into {sourceName}";
            
            string eql2 = "cross " +
                                   "JOIN SourceParaPruebas as t1 WHERE t1.PrimaryAccountNumber == \"9999941616073663_1\" " +
                                   "WITH SourceParaPruebas as t2 WHERE t2.PrimaryAccountNumber == \"9999941616073663_2\" " +
                                   "ON t1.AcquiringInstitutionIdentificationCode == t2.AcquiringInstitutionIdentificationCode " +
                                   "TIMEOUT '00:00:02' " +
                                   //"WHERE  t1.@event.Message.#1.#43 == \"Shell El RodeoGUATEMALA    GT\" " +
                                   $"SELECT (string)t1.PrimaryAccountNumber as c1, 1 as numeroXXX into {sourceName}";

            string command = $"use Database2; create source {sourceName} (c1 string, c2 string, numeroXXX int); create stream {oldStreamName} {{ {eql} }}; alter stream {oldStreamName} with query = {{ {eql2} }}";
            this.loginName = "sa";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        [TestMethod]
        public void AlterStreamAddProjectionColumns()
        {
            string oldStreamName = "oldStream";
            string newStreamName = "newStream";
            string sourceName = "newSource";
            string eql = "cross " +
                                   "JOIN SourceParaPruebas as t1 WHERE t1.PrimaryAccountNumber == \"9999941616073663_1\" " +
                                   "WITH SourceParaPruebas as t2 WHERE t2.PrimaryAccountNumber == \"9999941616073663_2\" " +
                                   "ON t1.AcquiringInstitutionIdentificationCode == t2.AcquiringInstitutionIdentificationCode " +
                                   "TIMEOUT '00:00:02' " +
                                   //"WHERE  t1.@event.Message.#1.#43 == \"Shell El RodeoGUATEMALA    GT\" " +
                                   $"SELECT (string)t1.PrimaryAccountNumber as c1, t2.PrimaryAccountNumber as c2, 1 as numeroXXX into {sourceName}";

            string eql2 = "cross " +
                                   "JOIN SourceParaPruebas as t1 WHERE t1.PrimaryAccountNumber == \"9999941616073663_1\" " +
                                   "WITH SourceParaPruebas as t2 WHERE t2.PrimaryAccountNumber == \"9999941616073663_2\" " +
                                   "ON t1.AcquiringInstitutionIdentificationCode == t2.AcquiringInstitutionIdentificationCode " +
                                   "TIMEOUT '00:00:02' " +
                                   //"WHERE  t1.@event.Message.#1.#43 == \"Shell El RodeoGUATEMALA    GT\" " +
                                   $"SELECT (string)t1.PrimaryAccountNumber as c1, t2.PrimaryAccountNumber as c2, 1 as numeroXXX, 2 as numeroYYY into {sourceName}";

            string command = $"use Database2; create source {sourceName} (c1 string, c2 string, numeroXXX int); create stream {oldStreamName} {{ {eql} }}; alter source {sourceName} add (numeroYYY int); alter stream {oldStreamName} with query = {{ {eql2} }}";
            this.loginName = "sa";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        [TestMethod]
        public void AlterStreamRenameProjectionColumns()
        {
            string oldStreamName = "oldStream";
            string newStreamName = "newStream";
            string sourceName = "newSource";
            string eql = "cross " +
                                   "JOIN SourceParaPruebas as t1 WHERE t1.PrimaryAccountNumber == \"9999941616073663_1\" " +
                                   "WITH SourceParaPruebas as t2 WHERE t2.PrimaryAccountNumber == \"9999941616073663_2\" " +
                                   "ON t1.AcquiringInstitutionIdentificationCode == t2.AcquiringInstitutionIdentificationCode " +
                                   "TIMEOUT '00:00:02' " +
                                   //"WHERE  t1.@event.Message.#1.#43 == \"Shell El RodeoGUATEMALA    GT\" " +
                                   $"SELECT (string)t1.PrimaryAccountNumber as c1, t2.PrimaryAccountNumber as c2, 1 as numeroXXX into {sourceName}";

            string eql2 = "cross " +
                                   "JOIN SourceParaPruebas as t1 WHERE t1.PrimaryAccountNumber == \"9999941616073663_1\" " +
                                   "WITH SourceParaPruebas as t2 WHERE t2.PrimaryAccountNumber == \"9999941616073663_2\" " +
                                   "ON t1.AcquiringInstitutionIdentificationCode == t2.AcquiringInstitutionIdentificationCode " +
                                   "TIMEOUT '00:00:02' " +
                                   //"WHERE  t1.@event.Message.#1.#43 == \"Shell El RodeoGUATEMALA    GT\" " +
                                   $"SELECT (string)t1.PrimaryAccountNumber as column1, t2.PrimaryAccountNumber as column2, 1 as column3 into {sourceName}";

            string command = $"use Database2; create source {sourceName} (column1 string, column2 string, column3 int, c1 string, c2 string, numeroXXX int); create stream {oldStreamName} {{ {eql} }}; alter stream {oldStreamName} with query = {{ {eql2} }}";
            this.loginName = "sa";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        [TestMethod]
        public void AlterStreamRenameProjectionColumnsSourceIncompatible()
        {
            string oldStreamName = "oldStream";
            string newStreamName = "newStream";
            string sourceName = "newSource";
            string eql = "cross " +
                                   "JOIN SourceInicial as t1 WHERE t1.PrimaryAccountNumber == \"9999941616073663_1\" " +
                                   "WITH SourceInicial as t2 WHERE t2.PrimaryAccountNumber == \"9999941616073663_2\" " +
                                   "ON t1.AcquiringInstitutionIdentificationCode == t2.AcquiringInstitutionIdentificationCode " +
                                   "TIMEOUT '00:00:02' " +
                                   //"WHERE  t1.@event.Message.#1.#43 == \"Shell El RodeoGUATEMALA    GT\" " +
                                   $"SELECT (string)t1.PrimaryAccountNumber as c1, t2.PrimaryAccountNumber as c2, 1 as numeroXXX into {sourceName}";

            string eql2 = "cross " +
                                   "JOIN SourceInicial as t1 WHERE t1.PrimaryAccountNumber == \"9999941616073663_1\" " +
                                   "WITH SourceInicial as t2 WHERE t2.PrimaryAccountNumber == \"9999941616073663_2\" " +
                                   "ON t1.AcquiringInstitutionIdentificationCode == t2.AcquiringInstitutionIdentificationCode " +
                                   "TIMEOUT '00:00:02' " +
                                   //"WHERE  t1.@event.Message.#1.#43 == \"Shell El RodeoGUATEMALA    GT\" " +
                                   $"SELECT (string)t1.PrimaryAccountNumber as column1, t2.PrimaryAccountNumber as column2, 1 as column3 into {sourceName}";

            string command = $"create source {sourceName} (column1 string, column3 int, c1 string, c2 object, numeroXXX int); create stream {oldStreamName} {{ {eql} }}; alter stream {oldStreamName} with query = {{ {eql2} }}";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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
                    catch (Exception e)
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void AlterStreamWithStatusOn()
        {
            string streamName = "newStream";
            string sourceName = "newSource";
            string eql = "cross " +
                                   "JOIN SourceParaPruebas as t1 WHERE t1.PrimaryAccountNumber == \"9999941616073663_1\" " +
                                   "WITH SourceParaPruebas as t2 WHERE t2.PrimaryAccountNumber == \"9999941616073663_2\" " +
                                   "ON t1.AcquiringInstitutionIdentificationCode == t2.AcquiringInstitutionIdentificationCode " +
                                   "TIMEOUT '00:00:02' " +
                                   //"WHERE  t1.@event.Message.#1.#43 == \"Shell El RodeoGUATEMALA    GT\" " +
                                   $"SELECT (string)t1.PrimaryAccountNumber as c1, t2.PrimaryAccountNumber as c2, 1 as numeroXXX into {sourceName} ";

            string command = $"use Database2; create source {sourceName} (c1 string, c2 string, numeroXXX int); create stream {streamName} {{ {eql} }} with status = off; alter stream {streamName} with status = on";
            this.loginName = "sa";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        [TestMethod]
        public void AlterStreamWithStatusOff()
        {
            string streamName = "newStream";
            string sourceName = "newSource";
            string eql = "cross " +
                                   "JOIN SourceParaPruebas as t1 WHERE t1.PrimaryAccountNumber == \"9999941616073663_1\" " +
                                   "WITH SourceParaPruebas as t2 WHERE t2.PrimaryAccountNumber == \"9999941616073663_2\" " +
                                   "ON t1.AcquiringInstitutionIdentificationCode == t2.AcquiringInstitutionIdentificationCode " +
                                   "TIMEOUT '00:00:02' " +
                                   //"WHERE  t1.@event.Message.#1.#43 == \"Shell El RodeoGUATEMALA    GT\" " +
                                   $"SELECT (string)t1.PrimaryAccountNumber as c1, t2.PrimaryAccountNumber as c2, 1 as numeroXXX into {sourceName} ";

            string command = $"use Database2; create source {sourceName} (c1 string, c2 string, numeroXXX int); create stream {streamName} {{ {eql} }} with status = on; alter stream {streamName} with status = off";
            this.loginName = "sa";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        [TestMethod]
        public void DropLogin()
        {
            string loginName = "login1";
            string password = "pass1234";
            string command = $"create login {loginName} with password = \"{password}\"; drop login {loginName}";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        [TestMethod]
        public void DropDatabase()
        {
            string databaseName = "newDatabase";
            string command = $"create database {databaseName}; drop database {databaseName}";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        [TestMethod]
        public void DropUser()
        {
            string userName = "newUser";
            string loginUserName = "AdminLogin2";
            string command = $"create user {userName} with login = {loginUserName}; drop user {userName}";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        [TestMethod]
        public void DropDatabaseRole()
        {
            string roleName = "role1";
            string command = $"create role {roleName}; drop role {roleName}";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        [TestMethod]
        public void DropSchema()
        {
            string schemaName = "newSchema";
            string command = $"create schema {schemaName}; drop schema {schemaName}";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        [TestMethod]
        public void DropSource()
        {
            string sourceName = "newSource";
            string command = $"create source {sourceName} (column1 int, column2 decimal, column3 string); drop source {sourceName}";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        [TestMethod]
        public void DropStream()
        {
            string sourceName = "sourceNameXX";
            string streamName = "newStream";
            string eql = "cross " +
                                   $"JOIN SourceParaPruebas as t1 WHERE t1.PrimaryAccountNumber == \"9999941616073663_1\" " +
                                   $"WITH SourceParaPruebas as t2 WHERE t2.PrimaryAccountNumber == \"9999941616073663_2\" " +
                                   "ON t1.AcquiringInstitutionIdentificationCode == t2.AcquiringInstitutionIdentificationCode " +
                                   "TIMEOUT '00:00:02' " +
                                   //"WHERE  t1.@event.Message.#1.#43 == \"Shell El RodeoGUATEMALA    GT\" " +
                                   $"SELECT (string)t1.PrimaryAccountNumber as c1, t2.PrimaryAccountNumber as c2, 1 as numeroXXX into {sourceName} ";

            string command = $"use Database2; create source {sourceName} (c1 string, c2 string, numeroXXX int); create stream {streamName} {{ {eql} }}; drop stream {streamName}";
            this.loginName = "sa";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        #region

        [TestMethod]
        public void TruncateSource()
        {
            string sourceName = "newSource";
            string command = $"create source {sourceName} (column1 int, column2 decimal, column3 string); truncate source {sourceName}";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
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

        #endregion

        #region otros

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
            string userNameThatCreateTheStream = "UserAux";
            string sourceNameTest = "source1234";
            string sourceNameTest2 = "sourceForInto";
            string databaseName = "Database2";
            string firstCommand = $"use {databaseName}; create source {sourceNameTest2} (c1 string, c3 string); create source {sourceNameTest} (MessageType string, RetrievalReferenceNumber string, PrimaryAccountNumber string, SourceTimestamp datetime, column1 int, column2 decimal, column3 string); grant create stream, read on source {sourceNameTest}, write on source {sourceNameTest2} to user {userNameThatCreateTheStream}";

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

            string secondCommand = $"use {databaseName}; create stream {newStreamName} {{\n{ eql }\n}}";
            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    this.loginName = "sa";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(firstCommand, kernel);

                    kernel = new StandardKernel();
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    kernel.Bind<ISourceTypeFactory>().ToConstructor(x => new SourceTypeFactory());
                    kernel.Bind<ISource>().ToConstructor(x => new ConcreteSource());
                    this.loginName = "LoginAux";
                    FirstLevelPipelineContext result2 = this.ProcessCommand(secondCommand, kernel);

                    Server server = dbContext.Servers.Single(x => x.ServerName == "Server1");
                    Database.Database db = dbContext.Databases.Single(x => x.ServerId == server.ServerId && x.DatabaseName == databaseName);
                    Database.Schema schema = dbContext.Schemas.Single(x => x.ServerId == server.ServerId && x.DatabaseId == db.DatabaseId && x.SchemaName == "dbo");
                    Database.Stream stream = dbContext.Streams.Single(x => x.ServerId == schema.ServerId && x.DatabaseId == schema.DatabaseId && x.SchemaId == schema.SchemaId && x.StreamName == newStreamName);
                    Assert.IsNotNull(stream);
                    Assert.AreEqual<string>(newStreamName, stream.StreamName);

                    Assert.IsTrue(stream.ProjectionColumns.Any(x => x.ColumnName == "c1" && x.ColumnType == typeof(string).AssemblyQualifiedName));
                    Assert.IsTrue(stream.ProjectionColumns.Any(x => x.ColumnName == "c3" && x.ColumnType == typeof(string).AssemblyQualifiedName));

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
        public void TestCreateDatabase()
        {
            string newDatabaseName = "Database1234";
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
            string newloginName = "Login123";
            string loginNameThatCreateTheLogin = "LoginAux";
            string firstCommand = $"grant alter any login to login {loginNameThatCreateTheLogin}";
            string secondCommand = $"create login {newloginName} with password = \"abc\", default_database = Database1";
            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(firstCommand, kernel);

                    this.loginName = loginNameThatCreateTheLogin;
                    FirstLevelPipelineContext result2 = this.ProcessCommand(secondCommand, kernel);
                    Server server = dbContext.Servers.Single(x => x.ServerName == "Server1");
                    Database.Database db = dbContext.Databases.Single(x => x.DatabaseName == "Database1");
                    Database.Login login = dbContext.Logins.Single(x => x.ServerId == server.ServerId && x.LoginName == newloginName);
                    Assert.IsNotNull(login);
                    Assert.AreEqual<string>(newloginName, login.LoginName);

                    Console.WriteLine();
                    tran.Rollback();
                }
            }
        }

        [TestMethod]
        public void TestCreateSource()
        {
            string newSource = "SourceInicial_nueva";
            string userNameThatCreateTheSource = "UserAux";
            string firstCommand = $"grant create source to user {userNameThatCreateTheSource}";
            string secondCommand = $"create source {newSource} (column1 int, column2 decimal, column3 string)";
            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    SpaceAssemblyBuilder sasmBuilder = new SpaceAssemblyBuilder("Test");
                    AssemblyBuilder asmBuilder = sasmBuilder.CreateAssemblyBuilder();
                    SpaceModuleBuilder smodBuilder = new SpaceModuleBuilder(asmBuilder);
                    smodBuilder.CreateModuleBuilder();
                    kernel.Bind<AssemblyBuilder>().ToConstant(asmBuilder);

                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(firstCommand, kernel);

                    this.loginName = "LoginAux";
                    FirstLevelPipelineContext result2 = this.ProcessCommand(secondCommand, kernel);

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

        #endregion otros
    }
}
