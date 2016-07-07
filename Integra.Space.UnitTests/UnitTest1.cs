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

            GenericPipelineExecutor cpe = new GenericPipelineExecutor(pipeline);
            PipelineContext context = new PipelineContext(command);
            PipelineContext result = cpe.Execute(context);
            return result;
        }

        [TestMethod]
        public void Test1()
        {
            string command = "create source Source2";
            PipelineContext result1 = this.ProcessCommand(command);
            SpecificPipelineExecutor cpe = new SpecificPipelineExecutor(result1.Pipeline);

            IKernel kernel = new StandardKernel(new TestModule());

            List<Source> sources = new List<Source>();
            sources.Add(new Source(Guid.NewGuid(), "Source2"));

            kernel.Bind<IRepository<Source>>()
                .To<SourceCacheRepository>()
                .WithConstructorArgument<List<Source>>(sources);
            
            PipelineExecutionCommandContext context = new PipelineExecutionCommandContext(result1.Command, kernel);
            PipelineExecutionCommandContext result2 = cpe.Execute(context);

            CacheRepositoryBase<Source> sr = kernel.Get<SourceCacheRepository>();
            Assert.IsNotNull(sr.FindByName("Source1"));

            Console.WriteLine();
        }
    }
}
