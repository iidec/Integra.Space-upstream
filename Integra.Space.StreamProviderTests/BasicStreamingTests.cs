using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orleans;
using Orleans.Runtime;
using Orleans.TestingHost;
using Orleans.TestingHost.Utils;
using System;
using System.IO;
using System.Threading.Tasks;
using Integra.Space.StreamProvider.TestGrainInterfaces;

namespace Integra.Space.StreamProviderTests
{
	[DeploymentItem("OrleansConfigurationForStreamingUnitTests.xml")]
	[DeploymentItem("OrleansProviders.dll")]
	[DeploymentItem("Integra.Space.StreamProvider.TestGrainInterfaces.dll")]
	[DeploymentItem("Integra.Space.StreamProvider.TestGrains.dll")]
	[DeploymentItem("Integra.Space.StreamProvider.dll")]
	[TestClass]
	public class BasicStreamingTests : UnitTestSiloHost
	{
		private const string SpaceStreamProviderName = "SpaceProvider";
		private const string StreamNamespace = "SpaceStreamNamespace";
		private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

		private Guid streamId;
		private string streamProvider;

		private static TestingSiloHost host;

		public BasicStreamingTests()
			: base(new TestingSiloOptions
			{
				StartFreshOrleans = false,
				StartSecondary = false,
				SiloConfigFile = new FileInfo("OrleansConfigurationForStreamingUnitTests.xml")
			})
		{
			host = this;
		}

		// Use ClassCleanup to run code after all tests in a class have run
		[ClassCleanup]
		public static void MyClassCleanup()
		{
			host.StopAllSilos();
		}

		[TestMethod]
		public async Task BasicStreamingTests_1()
		{
			this.logger.Info("************************ BasicStreamingTests_1 *********************************");
			this.streamId = Guid.NewGuid();
			streamProvider = SpaceStreamProviderName;
			await StreamingTests_Consumer_Producer(streamId, streamProvider);
		}

		private async Task StreamingTests_Consumer_Producer(Guid streamId, string streamProvider)
		{
			// consumer joins first, producer later
			var consumer = GrainClient.GrainFactory.GetGrain<IBasicStreaming_ConsumerGrain>(Guid.NewGuid());
			await consumer.BecomeConsumer(streamId, StreamNamespace, streamProvider);

			var producer = GrainClient.GrainFactory.GetGrain<IBasicStreaming_ProducerGrain>(Guid.NewGuid());
			await producer.BecomeProducer(streamId, StreamNamespace, streamProvider);

			await producer.StartPeriodicProducing();

			await Task.Delay(TimeSpan.FromMilliseconds(1000));

			await producer.StopPeriodicProducing();

			await TestingUtils.WaitUntilAsync(lastTry => CheckCounters(producer, consumer, lastTry), Timeout);

			await consumer.StopConsuming();
		}

		private async Task<bool> CheckCounters(IBasicStreaming_ProducerGrain producer, IBasicStreaming_ConsumerGrain consumer, bool assertIsTrue)
		{
			var numProduced = await producer.GetNumberProduced();
			var numConsumed = await consumer.GetNumberConsumed();
			logger.Info("CheckCounters: numProduced = {0}, numConsumed = {1}", numProduced, numConsumed);
			if (assertIsTrue)
			{
				Assert.AreEqual(numProduced, numConsumed, String.Format("numProduced = {0}, numConsumed = {1}", numProduced, numConsumed));
				return true;
			}
			else
			{
				if (numProduced == numConsumed)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
		}
	}
}
