using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orleans.Runtime;
using Moq;
using Integra.Space.StreamProvider;
using Orleans.Streams;
using System.Threading.Tasks;
using Integra.Space.StreamProvider.Redis;

namespace Integra.Space.StreamProviderTests
{
	[TestClass]
	public class SpaceQueueAdapterReceiverUnitTests
	{
		private TestContext testContext;
		private Logger logger;
		private Mock<IRedisBatchContainerFactory> batchContainerFactoryMock;
		private RedisConnectionString connectionString;
		private QueueId queueId;
		private Mock<RedisQueue> redisQueueMock;

		[TestInitialize]
		public void Initialize()
		{
			connectionString = new RedisConnectionString(TestContext.Properties["redisconnectionstring"].ToString());
			connectionString.Validate();
			Mock<Logger> loggerMock = new Mock<Logger>();
			batchContainerFactoryMock = new Mock<IRedisBatchContainerFactory>();
			logger = loggerMock.Object;
			queueId = QueueId.GetQueueId("NN", 0, 0);
			redisQueueMock = new Mock<RedisQueue>(connectionString.HostAddress, connectionString.HostPort, connectionString.Password, connectionString.DatabaseId, queueId.ToString());

		}

		public TestContext TestContext
		{
			get
			{
				return this.testContext;
			}
			set
			{
				this.testContext = value;
			}
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException), "Redis Queue cannot be null")]
		public void CtorRedisQueueNullTest()
		{
			SpaceQueueAdapterReceiver adapterReceiver = new SpaceQueueAdapterReceiver(null, this.queueId, this.connectionString, this.logger, this.batchContainerFactoryMock.Object);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException), "Queue Id cannot be null")]
		public void CtorQueueIdNullTest()
		{
			SpaceQueueAdapterReceiver adapterReceiver = new SpaceQueueAdapterReceiver(this.redisQueueMock.Object, null, this.connectionString, this.logger, this.batchContainerFactoryMock.Object);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException), "Connection string cannot be null")]
		public void CtorNullConnectionStringTest()
		{
			SpaceQueueAdapterReceiver adapterReceiver = new SpaceQueueAdapterReceiver(this.redisQueueMock.Object, this.queueId, null, this.logger, this.batchContainerFactoryMock.Object);
		}

		[TestMethod, TestCategory("UnitTest")]
		[ExpectedException(typeof(ArgumentNullException), "Invalid connection string")]
		public void CtorInvalidConnectionStringTest()
		{
			SpaceQueueAdapterReceiver adapterReceiver = new SpaceQueueAdapterReceiver(this.redisQueueMock.Object, this.queueId, new RedisConnectionString(null), this.logger, this.batchContainerFactoryMock.Object);
		}

		[TestMethod, TestCategory("UnitTest")]
		[ExpectedException(typeof(ArgumentNullException), "Logger cannot be null")]
		public void CtorTopicNullLoggerTest()
		{
			SpaceQueueAdapterReceiver adapterReceiver = new SpaceQueueAdapterReceiver(this.redisQueueMock.Object, this.queueId, this.connectionString, null, this.batchContainerFactoryMock.Object);
		}

		[TestMethod]
		public async Task InitializeGetLastOffsetTest()
		{
			SpaceQueueAdapterReceiver adapterReceiver = new SpaceQueueAdapterReceiver(this.redisQueueMock.Object, this.queueId, this.connectionString, this.logger, this.batchContainerFactoryMock.Object);
			await adapterReceiver.Initialize(TimeSpan.MaxValue);
		}

		[TestMethod]
		public async Task GetOneMessageQueueTest()
		{
			RedisMessage message = new RedisMessage(new byte[] { 0, 1, 2, 3 });
			Mock<IBatchContainer> batchContainerMock = new Mock<IBatchContainer>();
			redisQueueMock.Setup(x => x.DequeueAsync()).ReturnsAsync(message);
			this.batchContainerFactoryMock.Setup(x => x.FromRedis(It.IsAny<RedisMessage>())).Returns(batchContainerMock.Object);
			SpaceQueueAdapterReceiver adapterReceiver = new SpaceQueueAdapterReceiver(this.redisQueueMock.Object, this.queueId, this.connectionString, this.logger, this.batchContainerFactoryMock.Object);
			await adapterReceiver.Initialize(TimeSpan.MaxValue);
			var r = await adapterReceiver.GetQueueMessagesAsync(1);
			Assert.AreEqual(true, r.Count > 0);
		}

		[TestMethod]
		public async Task GetMessageEmptyQueueTest()
		{
			Mock<IBatchContainer> batchContainerMock = new Mock<IBatchContainer>();
			redisQueueMock.Setup(x => x.DequeueAsync()).ReturnsAsync(RedisMessage.Empty);
			this.batchContainerFactoryMock.Setup(x => x.FromRedis(It.IsAny<RedisMessage>())).Returns(batchContainerMock.Object);
			SpaceQueueAdapterReceiver adapterReceiver = new SpaceQueueAdapterReceiver(this.redisQueueMock.Object, this.queueId, this.connectionString, this.logger, this.batchContainerFactoryMock.Object);
			await adapterReceiver.Initialize(TimeSpan.MaxValue);
			var r = await adapterReceiver.GetQueueMessagesAsync(1);
			Assert.AreEqual(true, r.Count == 0);
		}
	}
}
