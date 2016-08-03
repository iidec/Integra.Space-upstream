using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Integra.Space.Pipeline;
using Ninject;
using Integra.Space.Cache;
using Integra.Space.Models;
using System.Collections.Generic;
using Integra.Space.Repos;
using Integra.Space.Common;
using Integra.Space.Database;

namespace Integra.Space.UnitTests
{
    [TestClass]
    public class CommandTests
    {
        private static Models.Schema schemaDePrueba = new Models.Schema(Guid.NewGuid(), "SchemaDePrueba");
        private User usuarioDelContexto = new User(Guid.NewGuid(), "admin", "admin", true);

        private PipelineContext ProcessCommand(string command, IKernel kernel)
        {
            CommandPipelineBuilder cpb = new CommandPipelineBuilder();
            Filter<PipelineContext, PipelineContext> pipeline = cpb.Build();

            PipelineExecutor cpe = new PipelineExecutor(pipeline);
            PipelineContext context = new PipelineContext(command, usuarioDelContexto, kernel);
            PipelineContext result = cpe.Execute(context);
            return result;
        }

        [TestMethod]
        public void TestCreateSource()
        {
            string command = "create source Source1";

            IKernel kernel = new StandardKernel();
            
            SystemContext systemContext = new SystemContext();
            systemContext.Sources.Add(new Models.Source(Guid.NewGuid(), "Source3", schemaDePrueba));
            systemContext.Users.Add(this.usuarioDelContexto);
            systemContext.Schemas.Add(schemaDePrueba);
            systemContext.PermissionsOverObjectType.Add(new PermissionOverObjectType(this.usuarioDelContexto, Common.SystemObjectEnum.Source, (int)(PermissionsEnum.Create), 0, schemaDePrueba));
            systemContext.SystemRoles.Add(new SystemRole(SystemRolesEnum.SysAdmin));
            systemContext.SystemRoles.Add(new SystemRole(SystemRolesEnum.SysReader));
            systemContext.SystemRoles.Add(new SystemRole(SystemRolesEnum.SchemaCreator));

            kernel.Bind<PermissionCacheRepository<PermissionOverObjectType>>()
                .ToConstant<PermissionOverObjectTypeCacheRepository>(new PermissionOverObjectTypeCacheRepository(systemContext))
                ;

            kernel.Bind<PermissionCacheRepository<PermissionOverSpecificObject>>()
                .ToConstant<PermissionOverSpecificObjectCacheRepository>(new PermissionOverSpecificObjectCacheRepository(systemContext))
                ;

            kernel.Bind<SystemRepositoryBase<SystemRole>>()
                .ToConstant<SystemRoleCacheRepository>(new SystemRoleCacheRepository(systemContext));

            kernel.Bind<IRepository<Models.Source>>()
                .ToConstant<SourceCacheRepository>(new SourceCacheRepository(systemContext));

            kernel.Bind<IRepository<User>>()
                .ToConstant<UserCacheRepository>(new UserCacheRepository(systemContext))
                ;

            PipelineContext result1 = this.ProcessCommand(command, kernel);

            List<Models.Source> sr = systemContext.Sources;
            Models.Source source = sr.Find(x => x.Name == "Source1");
            Assert.IsNotNull(source);
            Assert.AreEqual<string>("Source1", source.Name);

            Console.WriteLine();
        }

        [TestMethod]
        public void TestDropSource()
        {
            string command = "drop source Source1";

            IKernel kernel = new StandardKernel();

            SystemContext cacheContext = new SystemContext();
            Models.Source sourceTest = new Models.Source(Guid.NewGuid(), "Source1", schemaDePrueba);
            cacheContext.Sources.Add(sourceTest);
            cacheContext.Users.Add(this.usuarioDelContexto);
            cacheContext.Schemas.Add(schemaDePrueba);
            cacheContext.PermissionsOverSpecificObject.Add(new PermissionOverSpecificObject(this.usuarioDelContexto, sourceTest, (int)(PermissionsEnum.Owner), 0));
            cacheContext.SystemRoles.Add(new SystemRole(SystemRolesEnum.SysAdmin));
            cacheContext.SystemRoles.Add(new SystemRole(SystemRolesEnum.SysReader));
            cacheContext.SystemRoles.Add(new SystemRole(SystemRolesEnum.SchemaCreator));

            kernel.Bind<PermissionCacheRepository<PermissionOverObjectType>>()
                .ToConstant<PermissionOverObjectTypeCacheRepository>(new PermissionOverObjectTypeCacheRepository(cacheContext))
                ;

            kernel.Bind<PermissionCacheRepository<PermissionOverSpecificObject>>()
                .ToConstant<PermissionOverSpecificObjectCacheRepository>(new PermissionOverSpecificObjectCacheRepository(cacheContext))
                ;

            kernel.Bind<SystemRepositoryBase<SystemRole>>()
                .ToConstant<SystemRoleCacheRepository>(new SystemRoleCacheRepository(cacheContext))
                ;

            kernel.Bind<IRepository<Models.Source>>()
                .ToConstant<SourceCacheRepository>(new SourceCacheRepository(cacheContext));

            kernel.Bind<IRepository<User>>()
                .ToConstant<UserCacheRepository>(new UserCacheRepository(cacheContext))
                ;

            PipelineContext result1 = this.ProcessCommand(command, kernel);

            List<Models.Source> sr = cacheContext.Sources;
            Models.Source source = sr.Find(x => x.Name == "Source1");
            Assert.IsNull(source);

            Console.WriteLine();
        }

        [TestMethod]
        public void TestCreateStream()
        {
            string eql = "cross " +
                                  "JOIN SpaceObservable1 as t1 WHERE t1.@event.Message.#0.#0 == \"0100\" " +
                                  "WITH SpaceObservable1 as t2 WHERE t2.@event.Message.#0.#0 == \"0110\" " +
                                  "ON (string)t1.@event.Message.#1.#0 == (string)t2.@event.Message.#1.#0 and (string)t1.@event.Message.#1.#1 == (string)t2.@event.Message.#1.#1 " +
                                  "TIMEOUT '00:00:01.5' " +
                                  "WHERE isnull(t2.@event.SourceTimestamp, '01/01/2017') - isnull(t1.@event.SourceTimestamp, '01/01/2016') <= '00:00:01' " +
                                  "SELECT " +
                                          "t1.@event.Message.#1.#0 as c1, " +
                                          "t2.@event.Message.#1.#0 as c3 ";

            string command = $"create stream Stream1 {{\n{ eql }\n}}";
            
            IKernel kernel = new StandardKernel();

            SystemContext cacheContext = new SystemContext();
            cacheContext.Users.Add(this.usuarioDelContexto);
            cacheContext.Schemas.Add(schemaDePrueba);
            cacheContext.PermissionsOverObjectType.Add(new PermissionOverObjectType(this.usuarioDelContexto, Common.SystemObjectEnum.Stream, (int)(PermissionsEnum.Create), 0, schemaDePrueba));
            cacheContext.SystemRoles.Add(new SystemRole(SystemRolesEnum.SysAdmin));
            cacheContext.SystemRoles.Add(new SystemRole(SystemRolesEnum.SysReader));
            cacheContext.SystemRoles.Add(new SystemRole(SystemRolesEnum.SchemaCreator));

            kernel.Bind<PermissionCacheRepository<PermissionOverObjectType>>()
                .ToConstant<PermissionOverObjectTypeCacheRepository>(new PermissionOverObjectTypeCacheRepository(cacheContext))
                ;

            kernel.Bind<PermissionCacheRepository<PermissionOverSpecificObject>>()
                .ToConstant<PermissionOverSpecificObjectCacheRepository>(new PermissionOverSpecificObjectCacheRepository(cacheContext))
                ;

            kernel.Bind<SystemRepositoryBase<SystemRole>>()
                .ToConstant<SystemRoleCacheRepository>(new SystemRoleCacheRepository(cacheContext))
                ;

            kernel.Bind<IRepository<Models.Stream>>()
                .ToConstant<StreamCacheRepository>(new StreamCacheRepository(cacheContext));
            
            kernel.Bind<IRepository<User>>()
                .ToConstant<UserCacheRepository>(new UserCacheRepository(cacheContext))
                ;

            PipelineContext result1 = this.ProcessCommand(command, kernel);

            IRepository<Models.Stream> sr = kernel.Get<IRepository<Models.Stream>>();
            Models.Stream stream = sr.FindByName("Stream1");
            Assert.IsNotNull(stream);
            Assert.AreEqual<string>("Stream1", stream.Name);
            Assert.AreEqual<string>(eql.Trim(), stream.Query);

            Console.WriteLine();
        }

        [TestMethod]
        public void CreateUser()
        {
            string command = "create user User1 password \"abc1234\" status enable";

            IKernel kernel = new StandardKernel();

            SystemContext cacheContext = new SystemContext();
            cacheContext.Users.Add(this.usuarioDelContexto);
            cacheContext.Users.Add(new User(Guid.NewGuid(), "User2", "abc1234", true));
            cacheContext.Schemas.Add(schemaDePrueba);
            cacheContext.PermissionsOverObjectType.Add(new PermissionOverObjectType(this.usuarioDelContexto, Common.SystemObjectEnum.User, (int)(PermissionsEnum.Create), 0, schemaDePrueba));
            cacheContext.SystemRoles.Add(new SystemRole(SystemRolesEnum.SysAdmin));
            cacheContext.SystemRoles.Add(new SystemRole(SystemRolesEnum.SysReader));
            cacheContext.SystemRoles.Add(new SystemRole(SystemRolesEnum.SchemaCreator));

            kernel.Bind<PermissionCacheRepository<PermissionOverObjectType>>()
                .ToConstant<PermissionOverObjectTypeCacheRepository>(new PermissionOverObjectTypeCacheRepository(cacheContext))
                ;

            kernel.Bind<PermissionCacheRepository<PermissionOverSpecificObject>>()
                .ToConstant<PermissionOverSpecificObjectCacheRepository>(new PermissionOverSpecificObjectCacheRepository(cacheContext))
                ;

            kernel.Bind<SystemRepositoryBase<SystemRole>>()
                .ToConstant<SystemRoleCacheRepository>(new SystemRoleCacheRepository(cacheContext))
                ;

            kernel.Bind<IRepository<User>>()
                .ToConstant<UserCacheRepository>(new UserCacheRepository(cacheContext));

            kernel.Bind<PermissionCacheRepository<PermissionOverObjectType>>()
                .ToConstant<PermissionOverObjectTypeCacheRepository>(new PermissionOverObjectTypeCacheRepository(cacheContext))
                ;
                        
            PipelineContext result1 = this.ProcessCommand(command, kernel);

            IRepository<User> sr = kernel.Get<IRepository<User>>();
            User user = sr.FindByName("User1");
            Assert.IsNotNull(user);
            Assert.AreEqual<string>("User1", user.Name);
            Assert.IsTrue(user.Enable);
            Assert.AreEqual<string>("abc1234", user.Password);

            Console.WriteLine();
        }
        
        [TestMethod]
        public void AlterUser()
        {
            string command = "alter user User1 password \"abc1234\" status enable";

            IKernel kernel = new StandardKernel();

            SystemContext cacheContext = new SystemContext();
            cacheContext.Users.Add(this.usuarioDelContexto);
            User userToUpdate = new User(Guid.NewGuid(), "User1", "abc", false);
            cacheContext.Users.Add(userToUpdate);
            cacheContext.Schemas.Add(schemaDePrueba);
            cacheContext.PermissionsOverSpecificObject.Add(new PermissionOverSpecificObject(this.usuarioDelContexto, userToUpdate, (int)(PermissionsEnum.Alter), 0));

            kernel.Bind<IRepository<User>>()
                .ToConstant<UserCacheRepository>(new UserCacheRepository(cacheContext));

            kernel.Bind<PermissionCacheRepository<PermissionOverSpecificObject>>()
                .ToConstant<PermissionOverSpecificObjectCacheRepository>(new PermissionOverSpecificObjectCacheRepository(cacheContext))
                ;

            PipelineContext result1 = this.ProcessCommand(command, kernel);

            IRepository<User> sr = kernel.Get<IRepository<User>>();
            User user = sr.FindByName("User1");
            Assert.IsTrue(user.Enable);
            Assert.AreEqual<string>("abc1234", user.Password);

            Console.WriteLine();
        }

        [TestMethod]
        public void GrantPermission()
        {
            string eql = "cross " +
                                  "JOIN SpaceObservable1 as t1 WHERE t1.@event.Message.#0.#0 == \"0100\" " +
                                  "WITH SpaceObservable1 as t2 WHERE t2.@event.Message.#0.#0 == \"0110\" " +
                                  "ON (string)t1.@event.Message.#1.#0 == (string)t2.@event.Message.#1.#0 and (string)t1.@event.Message.#1.#1 == (string)t2.@event.Message.#1.#1 " +
                                  "TIMEOUT '00:00:01.5' " +
                                  "WHERE isnull(t2.@event.SourceTimestamp, '01/01/2017') - isnull(t1.@event.SourceTimestamp, '01/01/2016') <= '00:00:01' " +
                                  "SELECT " +
                                          "t1.@event.Message.#1.#0 as c1, " +
                                          "t2.@event.Message.#1.#0 as c3 ";

            string command = "grant create stream, read stream Stream1 to user User1";

            IKernel kernel = new StandardKernel();

            SystemContext cacheContext = new SystemContext();
            cacheContext.Streams.Add(new Models.Stream(Guid.NewGuid(), "Stream1", eql, schemaDePrueba));
            cacheContext.Users.Add(new User(Guid.NewGuid(), "User1", "abc", true));
            cacheContext.Users.Add(this.usuarioDelContexto);
            cacheContext.Schemas.Add(schemaDePrueba);
            cacheContext.PermissionsOverObjectType.Add(new PermissionOverObjectType(this.usuarioDelContexto, SystemObjectEnum.Stream, (int)PermissionsEnum.Create, 0, schemaDePrueba));

            kernel.Bind<IRepository<User>>()
                .ToConstant<UserCacheRepository>(new UserCacheRepository(cacheContext));

            kernel.Bind<IRepository<Models.Stream>>()
                .ToConstant<StreamCacheRepository>(new StreamCacheRepository(cacheContext))
                ;

            kernel.Bind<PermissionCacheRepository<PermissionOverObjectType>>()
                .ToConstant<PermissionOverObjectTypeCacheRepository>(new PermissionOverObjectTypeCacheRepository(cacheContext))
                ;

            PipelineContext result1 = this.ProcessCommand(command, kernel);

            IRepository<PermissionAssigned> sr = kernel.Get<IRepository<PermissionAssigned>>();

            Console.WriteLine();
        }
                
        [TestMethod]
        public void DenyPermission()
        {
            string command = "deny create stream to user User1";

            IKernel kernel = new StandardKernel();

            SystemContext cacheContext = new SystemContext();
            cacheContext.Users.Add(this.usuarioDelContexto);
            cacheContext.Users.Add(new User(Guid.NewGuid(), "User1", "abc", true));
            cacheContext.Schemas.Add(schemaDePrueba);
            cacheContext.PermissionsOverObjectType.Add(new PermissionOverObjectType(this.usuarioDelContexto, SystemObjectEnum.Stream, (int)PermissionsEnum.Owner, 0, schemaDePrueba));

            kernel.Bind<IRepository<User>>()
                .ToConstant<UserCacheRepository>(new UserCacheRepository(cacheContext));

            kernel.Bind<PermissionCacheRepository<PermissionOverObjectType>>()
                .ToConstant<PermissionOverObjectTypeCacheRepository>(new PermissionOverObjectTypeCacheRepository(cacheContext))
                ;
            
            PipelineContext result1 = this.ProcessCommand(command, kernel);

            IRepository<PermissionAssigned> sr = kernel.Get<IRepository<PermissionAssigned>>();

            Console.WriteLine();
        }

        [TestMethod]
        public void RevokePermission()
        {
            string command = "revoke create stream, create source to user User1";

            IKernel kernel = new StandardKernel();

            SystemContext cacheContext = new SystemContext();
            cacheContext.Users.Add(this.usuarioDelContexto);
            cacheContext.Users.Add(new User(Guid.NewGuid(), "User1", "abc", true));
            cacheContext.Schemas.Add(schemaDePrueba);
            cacheContext.PermissionsOverObjectType.Add(new PermissionOverObjectType(this.usuarioDelContexto, Common.SystemObjectEnum.Source, 1, 0, schemaDePrueba));
            cacheContext.PermissionsOverObjectType.Add(new PermissionOverObjectType(this.usuarioDelContexto, Common.SystemObjectEnum.Stream, 2, 0, schemaDePrueba));

            kernel.Bind<IRepository<User>>()
                .ToConstant<UserCacheRepository>(new UserCacheRepository(cacheContext));

            kernel.Bind<PermissionCacheRepository<PermissionOverObjectType>>()
                .ToConstant<PermissionOverObjectTypeCacheRepository>(new PermissionOverObjectTypeCacheRepository(cacheContext))
                ;

            PipelineContext result1 = this.ProcessCommand(command, kernel);

            IRepository<PermissionAssigned> sr = kernel.Get<IRepository<PermissionAssigned>>();

            Console.WriteLine();
        }

        [TestMethod]
        public void AddSecureObjectsToRole()
        {
            string command = "add user User1, user User2, user User3, user User4 to role Role1";

            IKernel kernel = new StandardKernel();

            SystemContext cacheContext = new SystemContext();
            cacheContext.Users.Add(this.usuarioDelContexto);
            cacheContext.Users.Add(new User(Guid.NewGuid(), "User1", "abc", true));
            cacheContext.Users.Add(new User(Guid.NewGuid(), "User2", "abc", true));
            cacheContext.Users.Add(new User(Guid.NewGuid(), "User3", "abc", true));
            cacheContext.Users.Add(new User(Guid.NewGuid(), "User4", "abc", true));

            cacheContext.Roles.Add(new Role(Guid.NewGuid(), "Role1"));

            cacheContext.SystemRoles.Add(new SystemRole(SystemRolesEnum.SysAdmin));
            cacheContext.SystemRoles.Add(new SystemRole(SystemRolesEnum.SysReader));
            cacheContext.SystemRoles.Add(new SystemRole(SystemRolesEnum.SchemaCreator));

            kernel.Bind<PermissionCacheRepository<PermissionOverObjectType>>()
                .ToConstant<PermissionOverObjectTypeCacheRepository>(new PermissionOverObjectTypeCacheRepository(cacheContext))
                ;

            kernel.Bind<PermissionCacheRepository<PermissionOverSpecificObject>>()
                .ToConstant<PermissionOverSpecificObjectCacheRepository>(new PermissionOverSpecificObjectCacheRepository(cacheContext))
                ;

            kernel.Bind<SystemRepositoryBase<SystemRole>>()
                .ToConstant<SystemRoleCacheRepository>(new SystemRoleCacheRepository(cacheContext))
                ;

            kernel.Bind<IRepository<UserXRole>>()
                .ToConstant<UserXRoleCacheRepository>(new UserXRoleCacheRepository(cacheContext));

            kernel.Bind<IRepository<User>>()
                .ToConstant<UserCacheRepository>(new UserCacheRepository(cacheContext));

            kernel.Bind<IRepository<Role>>()
                .ToConstant<RoleCacheRepository>(new RoleCacheRepository(cacheContext));
            
            PipelineContext result1 = this.ProcessCommand(command, kernel);

            IRepository<UserXRole> r = kernel.Get<IRepository<UserXRole>>();

            Console.WriteLine();
        }
    }
}
