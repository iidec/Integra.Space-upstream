using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Integra.Space.Pipeline;
using Ninject;
using Integra.Space.Cache;
using Integra.Space.Models;
using Ninject.Parameters;
using System.Collections.Generic;
using Integra.Space.Repos;

namespace Integra.Space.UnitTests
{
    [TestClass]
    public class UnitTest1
    {
        private PipelineContext ProcessCommand(string command)
        {
            CommandPipelineBuilder cpb = new CommandPipelineBuilder();
            Filter<PipelineContext, PipelineContext> pipeline = cpb.Build();

            FirstPipelineExecutor cpe = new FirstPipelineExecutor(pipeline);
            PipelineContext context = new PipelineContext(command);
            PipelineContext result = cpe.Execute(context);
            return result;
        }

        [TestMethod]
        public void TestCreateSource()
        {
            string command = "create source Source1";

            IKernel kernel = new StandardKernel();

            kernel.Bind<IRepository<Source>>()
                .To<SourceCacheRepository>();

            kernel.Bind<CacheContext>()
                .ToSelf()
                .InSingletonScope()
                ;

            kernel.Get<CacheContext>().Sources.Add(new Source(Guid.NewGuid(), "Source3"));
            
            PipelineContext result1 = this.ProcessCommand(command);
            PipelineExecutionCommandContext context = new PipelineExecutionCommandContext(result1.Command, kernel);
            SpecificPipelineExecutor cpe = new SpecificPipelineExecutor(result1.Pipeline);
            PipelineExecutionCommandContext result2 = cpe.Execute(context);

            List<Source> sr = kernel.Get<CacheContext>().Sources;
            Source source = sr.Find(x => x.Identifier == "Source1");
            Assert.IsNotNull(source);
            Assert.AreEqual<string>("Source1", source.Identifier);

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
            PipelineContext result1 = this.ProcessCommand(command);
            
            IKernel kernel = new StandardKernel();

            kernel.Bind<IRepository<Stream>>()
                .To<StreamCacheRepository>();

            kernel.Bind<CacheContext>()
                .ToSelf()
                .InSingletonScope()
                ;

            PipelineExecutionCommandContext context = new PipelineExecutionCommandContext(result1.Command, kernel);
            SpecificPipelineExecutor cpe = new SpecificPipelineExecutor(result1.Pipeline);
            PipelineExecutionCommandContext result2 = cpe.Execute(context);

            IRepository<Stream> sr = kernel.Get<IRepository<Stream>>();
            Stream stream = sr.FindByName("Stream1");
            Assert.IsNotNull(stream);
            Assert.AreEqual<string>("Stream1", stream.Identifier);
            Assert.AreEqual<string>(eql.Trim(), stream.Query);

            Console.WriteLine();
        }

        [TestMethod]
        public void CreateUser()
        {
            string command = "create user User1 password \"abc1234\" status enable";

            IKernel kernel = new StandardKernel();

            List<User> users = new List<User>();
            users.Add(new User(Guid.NewGuid(), "User2", "abc1234", true));

            kernel.Bind<IRepository<User>>()
                .To<UserCacheRepository>();

            kernel.Bind<CacheContext>()
                .ToSelf()
                .InSingletonScope()
                ;

            PipelineContext result1 = this.ProcessCommand(command);
            PipelineExecutionCommandContext context = new PipelineExecutionCommandContext(result1.Command, kernel);
            SpecificPipelineExecutor cpe = new SpecificPipelineExecutor(result1.Pipeline);
            PipelineExecutionCommandContext result2 = cpe.Execute(context);

            IRepository<User> sr = kernel.Get<IRepository<User>>();
            User user = sr.FindByName("User1");
            Assert.IsNotNull(user);
            Assert.AreEqual<string>("User1", user.Identifier);
            Assert.IsTrue(user.Enable);
            Assert.AreEqual<string>("abc1234", user.Password);

            Console.WriteLine();
        }
        
        [TestMethod]
        public void AlterUser()
        {
            string command = "alter user User1 password \"abc1234\" status enable";

            IKernel kernel = new StandardKernel();
            
            kernel.Bind<CacheContext>()
                .ToSelf()
                .InSingletonScope()
                ;

            kernel.Bind<IRepository<User>>()
                .To<UserCacheRepository>();

            kernel.Get<CacheContext>().Users.Add(new User(Guid.NewGuid(), "User1", "abc", false));

            PipelineContext result1 = this.ProcessCommand(command);
            PipelineExecutionCommandContext context = new PipelineExecutionCommandContext(result1.Command, kernel);
            SpecificPipelineExecutor cpe = new SpecificPipelineExecutor(result1.Pipeline);
            PipelineExecutionCommandContext result2 = cpe.Execute(context);

            IRepository<User> sr = kernel.Get<IRepository<User>>();
            User user = sr.FindByName("User1");
            Assert.IsTrue(user.Enable);
            Assert.AreEqual<string>("abc1234", user.Password);

            Console.WriteLine();
        }

        [TestMethod]
        public void GrantPermission()
        {
            string command = "grant create stream to user User1";

            IKernel kernel = new StandardKernel();

            kernel.Bind<CacheContext>()
                .ToSelf()
                .InSingletonScope()
                ;

            kernel.Bind<IRepository<User>>()
                .To<UserCacheRepository>();

            kernel.Get<CacheContext>().Users.Add(new User(Guid.NewGuid(), "User1", "abc", false));
            
            kernel.Bind<IRepository<Permission>>()
                .To<PermissionCacheRepository>();

            PipelineContext result1 = this.ProcessCommand(command);
            PipelineExecutionCommandContext context = new PipelineExecutionCommandContext(result1.Command, kernel);
            SpecificPipelineExecutor cpe = new SpecificPipelineExecutor(result1.Pipeline);
            PipelineExecutionCommandContext result2 = cpe.Execute(context);

            IRepository<Permission> sr = kernel.Get<IRepository<Permission>>();

            Console.WriteLine();
        }
    }
}
