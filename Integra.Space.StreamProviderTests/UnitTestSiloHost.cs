using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orleans.TestingHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integra.Space.StreamProviderTests
{
	public class UnitTestSiloHost : TestingSiloHost
	{
		public UnitTestSiloHost() // : base()
		{
		}
		public UnitTestSiloHost(TestingSiloOptions siloOptions) : base(siloOptions)
		{
		}
		public UnitTestSiloHost(TestingSiloOptions siloOptions, TestingClientOptions clientOptions) : base(siloOptions, clientOptions)
		{
		}

		protected static string DumpTestContext(TestContext context)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendFormat(@"TestName={0}", context.TestName).AppendLine();
			sb.AppendFormat(@"FullyQualifiedTestClassName={0}", context.FullyQualifiedTestClassName).AppendLine();
			sb.AppendFormat(@"CurrentTestOutcome={0}", context.CurrentTestOutcome).AppendLine();
			sb.AppendFormat(@"DeploymentDirectory={0}", context.DeploymentDirectory).AppendLine();
			sb.AppendFormat(@"TestRunDirectory={0}", context.TestRunDirectory).AppendLine();
			sb.AppendFormat(@"TestResultsDirectory={0}", context.TestResultsDirectory).AppendLine();
			sb.AppendFormat(@"ResultsDirectory={0}", context.ResultsDirectory).AppendLine();
			sb.AppendFormat(@"TestRunResultsDirectory={0}", context.TestRunResultsDirectory).AppendLine();
			sb.AppendFormat(@"Properties=[ ");
			foreach (var key in context.Properties.Keys)
			{
				sb.AppendFormat(@"{0}={1} ", key, context.Properties[key]);
			}
			sb.AppendFormat(@" ]").AppendLine();
			return sb.ToString();
		}
	}
}
