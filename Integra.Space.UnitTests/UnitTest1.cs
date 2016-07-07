using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Integra.Space.Pipeline;
using Integra.Space.Common.CommandContext;
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
        private ExecutionPipelineNode<PipelineCommandContext, PipelineCommandContext> ProcessCommand(string command)
        {
            CommandPipelineBuilder cpb = new CommandPipelineBuilder();
            Filter<string, ExecutionPipelineNode<PipelineCommandContext, PipelineCommandContext>> pipeline = cpb.Build();
            CommandPipelineExecutor<string, ExecutionPipelineNode<PipelineCommandContext, PipelineCommandContext>> cpe = new CommandPipelineExecutor<string, ExecutionPipelineNode<PipelineCommandContext, PipelineCommandContext>>(pipeline);
            ExecutionPipelineNode<PipelineCommandContext, PipelineCommandContext>  result = cpe.Execute(command);
            return result;
        }

        [TestMethod]
        public void Test1()
        {
            string command = "create source Source1";
            ExecutionPipelineNode<PipelineCommandContext, PipelineCommandContext> result1 = this.ProcessCommand(command);
            CommandPipelineExecutor<PipelineCommandContext, PipelineCommandContext> cpe = new CommandPipelineExecutor<PipelineCommandContext, PipelineCommandContext>(result1.Pipeline);

            IKernel kernel = new StandardKernel(new TestModule());

            List<Source> sources = new List<Source>();
            sources.Add(new Source(Guid.NewGuid(), "Source2"));

            kernel.Bind<IRepository<Source>>()
                .To<SourceCacheRepository>()
                .WithConstructorArgument<List<Source>>(sources);
            var x = kernel.Get<SourceCacheRepository>();

            PipelineCommandContext context = new PipelineCommandContext(result1.Command, kernel);
            PipelineCommandContext result2 = cpe.Execute(context);
            CacheRepositoryBase<Source> sr = kernel.Get<SourceCacheRepository>();

            Assert.IsNotNull(sr.FindByName("Source1"));

            Console.WriteLine();
        }
    }
}
