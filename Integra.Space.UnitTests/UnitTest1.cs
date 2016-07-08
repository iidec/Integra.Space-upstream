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
            PipelineContext result1 = this.ProcessCommand(command);
            SpecificPipelineExecutor cpe = new SpecificPipelineExecutor(result1.Pipeline);

            IKernel kernel = new StandardKernel();
            
            List<Source> sources = new List<Source>();
            sources.Add(new Source(Guid.NewGuid(), "Source3"));

            kernel.Bind<IRepository<Source>>()
                .To<SourceCacheRepository>()
                .WithConstructorArgument<List<Source>>(sources);
            
            PipelineExecutionCommandContext context = new PipelineExecutionCommandContext(result1.Command, kernel);
            PipelineExecutionCommandContext result2 = cpe.Execute(context);

            IRepository<Source> sr = kernel.Get<IRepository<Source>>();
            Assert.IsNotNull(sr.FindByName("Source1"));

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
            SpecificPipelineExecutor cpe = new SpecificPipelineExecutor(result1.Pipeline);

            IKernel kernel = new StandardKernel();

            List<Stream> streams = new List<Stream>();

            kernel.Bind<IRepository<Stream>>()
                .To<StreamCacheRepository>()
                .WithConstructorArgument<List<Stream>>(streams);

            PipelineExecutionCommandContext context = new PipelineExecutionCommandContext(result1.Command, kernel);
            PipelineExecutionCommandContext result2 = cpe.Execute(context);

            IRepository<Stream> sr = kernel.Get<IRepository<Stream>>();
            Assert.IsNotNull(sr.FindByName("Stream1"));

            Console.WriteLine();
        }
    }
}
