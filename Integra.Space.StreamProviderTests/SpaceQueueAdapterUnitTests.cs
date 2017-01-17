using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orleans.Streams;
using Moq;
using Orleans.Runtime;
using Integra.Space.StreamProvider;
using System.Threading.Tasks;
using System.Collections.Generic;
using Integra.Space.StreamProvider.Redis;

namespace Integra.Space.StreamProviderTests
{
	[TestClass]
	public class SpaceQueueAdapterUnitTests
	{
		private string providerName = "Test";
		private readonly HashRingBasedStreamQueueMapper streamQueueMapper;
		private readonly Logger logger;
		private readonly RedisConnectionString connectionString;
		Mock<IRedisBatchContainerFactory> batchContainerFactoryMock;

		public SpaceQueueAdapterUnitTests()
		{
			Mock<Logger> loggerMock = new Mock<Logger>();
			batchContainerFactoryMock = new Mock<IRedisBatchContainerFactory>();

			logger = loggerMock.Object;
			streamQueueMapper = new HashRingBasedStreamQueueMapper(1, "NN");
			connectionString = new RedisConnectionString("HostAddress=54.172.20.246;HostPort=12574;Password=redizz;DatabaseId=0");
		}

		[TestMethod, TestCategory("UnitTest")]
		[ExpectedException(typeof(ArgumentNullException), "Queue Mapper cannot be null")]
		public void CtorNullStreamQueueMapperTest()
		{
			SpaceQueueAdapter adapter = new SpaceQueueAdapter(null, providerName, connectionString, logger, batchContainerFactoryMock.Object);
		}

		[TestMethod, TestCategory("UnitTest")]
		[ExpectedException(typeof(ArgumentNullException), "Provider Name cannot be null or empty")]
		public void CtorEmptyProviderNameTest()
		{
			SpaceQueueAdapter adapter = new SpaceQueueAdapter(streamQueueMapper, null, connectionString, logger, batchContainerFactoryMock.Object);
		}

		[TestMethod, TestCategory("UnitTest")]
		[ExpectedException(typeof(ArgumentNullException), "Connection string cannot be null")]
		public void CtorNullConnectionStringTest()
		{
			SpaceQueueAdapter adapter = new SpaceQueueAdapter(streamQueueMapper, providerName, null, logger, batchContainerFactoryMock.Object);
		}

		[TestMethod, TestCategory("UnitTest")]
		[ExpectedException(typeof(ArgumentNullException), "Invalid connection string")]
		public void CtorInvalidConnectionStringTest()
		{
			SpaceQueueAdapter adapter = new SpaceQueueAdapter(streamQueueMapper, providerName, new RedisConnectionString(null), logger, batchContainerFactoryMock.Object);
		}

		[TestMethod, TestCategory("UnitTest")]
		[ExpectedException(typeof(ArgumentNullException), "Logger cannot be null")]
		public void CtorTopicNullLoggerTest()
		{
			SpaceQueueAdapter adapter = new SpaceQueueAdapter(streamQueueMapper, providerName, connectionString, null, batchContainerFactoryMock.Object);
		}

		[TestMethod, TestCategory("UnitTest")]
		[ExpectedException(typeof(ArgumentNullException), "Container batch factory cannot be null")]
		public void CtorNullBatchFactoryTest()
		{
			SpaceQueueAdapter adapter = new SpaceQueueAdapter(streamQueueMapper, providerName, connectionString, logger, null);
		}

		[TestMethod, TestCategory("UnitTest")]
		[ExpectedException(typeof(Exception), "Invalid Redis Authentication")]
		public async Task RedisConnectionTest()
		{
			RedisConnectionString connectionString = new RedisConnectionString("HostAddress=54.172.20.246;HostPort=12574;Password=masterzz;DatabaseId=0");
			Dictionary<string, object> requestContext = new Dictionary<string, object>();
			batchContainerFactoryMock.Setup(x => x.ToRedisMessage<int>(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<IEnumerable<int>>(), new RedisEventSequenceToken(0), requestContext)).Returns(new RedisMessage(new byte[] { 0, 1, 2, 3 }));
			SpaceQueueAdapter adapter = new SpaceQueueAdapter(streamQueueMapper, providerName, connectionString, logger, batchContainerFactoryMock.Object);
			await adapter.QueueMessageBatchAsync(Guid.NewGuid(), "Test", new List<int>() { 1, 2, 3, 4 }, null, requestContext);
		}

		[TestMethod, TestCategory("UnitTest")]
		public async Task QueueMessageBatchAsyncSimpleTest()
		{
			Dictionary<string, object> requestContext = new Dictionary<string, object>();
			batchContainerFactoryMock.Setup(x => x.ToRedisMessage<int>(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<IEnumerable<int>>(), It.IsAny<RedisEventSequenceToken>(), requestContext)).Returns(new RedisMessage(new byte[] { 0, 1, 2, 3 }));
			SpaceQueueAdapter adapter = new SpaceQueueAdapter(streamQueueMapper, providerName, connectionString, logger, batchContainerFactoryMock.Object);
			await adapter.QueueMessageBatchAsync(Guid.NewGuid(), "Test", (IEnumerable<int>)new int[] { 1, 2, 3, 4 }, null, requestContext);
		}
	}
}
