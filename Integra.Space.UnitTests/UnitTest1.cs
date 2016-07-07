using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Integra.Space.Pipeline;
using Integra.Space.Common.CommandContext;
using Ninject;
using Integra.Space.UnitTests.NinjectModules;
using Integra.Space.Cache;
using Integra.Space.Models;

namespace Integra.Space.UnitTests
{
    [TestClass]
    public class UnitTest1
    {
        private ExecutionPipelineNode<PipelineCommandContext, PipelineCommandContext> Process(string command)
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
            ExecutionPipelineNode<PipelineCommandContext, PipelineCommandContext> result1 = this.Process(command);
            CommandPipelineExecutor<PipelineCommandContext, PipelineCommandContext> cpe = new CommandPipelineExecutor<PipelineCommandContext, PipelineCommandContext>(result1.Pipeline);

            IKernel kernel = new StandardKernel(new TestModule());
            PipelineCommandContext context = new PipelineCommandContext(result1.Command, kernel);
            PipelineCommandContext result2 = cpe.Execute(context);
            ICacheRepository<Source> sr = (SourceRepository)kernel.GetService(typeof(SourceRepository));
            Source sourceCreated = sr.FindByName("Source1");

            Console.WriteLine();
        }
    }
}
