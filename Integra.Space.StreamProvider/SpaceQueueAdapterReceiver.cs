using Integra.Space.StreamProvider.Redis;
using Orleans;
using Orleans.Runtime;
using Orleans.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integra.Space.StreamProvider
{
	/// <summary>
	/// Implement MyQueueAdapterReceiver class that implements the IQueueAdapterReceiver,
	/// which is an interfaces that manages access to one queue (one queue partition).
	/// In addition to initialization and shutdown, it basically provides one method:
	/// retrieve up to maxCount messages from the queue.
	/// </summary>
	public class SpaceQueueAdapterReceiver : IQueueAdapterReceiver
	{
		QueueId queueId;
		RedisQueue queue;
		Logger logger;
		RedisConnectionString connectionString;
		IRedisBatchContainerFactory batchContainerFactory;

		private static readonly IList<IBatchContainer> EmptyBatchContainer = Enumerable.Empty<IBatchContainer>().ToList();

		public SpaceQueueAdapterReceiver(RedisQueue queue, QueueId queueId, RedisConnectionString connectionString, Logger logger, IRedisBatchContainerFactory batchContainerFactory)
		{
			if (queue == null) throw new ArgumentNullException(nameof(queue));
			if (queueId == null) throw new ArgumentNullException(nameof(queueId));
			if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));
			if (batchContainerFactory == null) throw new ArgumentNullException(nameof(batchContainerFactory));
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			connectionString.Validate(); // Esto puede dar generar un excepcion si esta mal escrito el CN

			this.queue = queue;
			this.queueId = queueId;
			this.connectionString = connectionString;
			this.logger = logger;
			this.batchContainerFactory = batchContainerFactory;
		}

		public Task Initialize(TimeSpan timeout)
		{
			//this.logger.Verbose("RedisQueueAdapterReceiver - Initialized with HostAddress: {0} HostPort: {1} Password: {2} DatabaseId: {3} QueueId: {4}", connectionString.HostAddress, connectionString.HostPort, connectionString.Password, connectionString.DatabaseId, this.queueId.ToString());
			//try
			//{
			//	this.queue = new RedisQueue2(connectionString.HostAddress, connectionString.HostPort, connectionString.Password, connectionString.DatabaseId, this.queueId.ToString());
			//}
			//catch(Exception e)
			//{
			//	this.logger.Error(0, "RedisQueueAdapterReceiver - Initialization exception", e);
			//	throw;
			//}

			return TaskDone.Done;
		}

		public async Task<IList<IBatchContainer>> GetQueueMessagesAsync(int maxCount)
		{
			this.logger.Verbose("GetQueueMessagesAsync...");
			RedisMessage message = await this.queue.DequeueAsync();
			if (message == RedisMessage.Empty)
			{
				this.logger.Verbose("GetQueueMessagesAsync EMPTY...");
				//return SpaceQueueAdapterReceiver.EmptyBatchContainer;
				return new List<IBatchContainer>();
			}

			IBatchContainer batchContainer = null;

			try
			{
				batchContainer = this.batchContainerFactory.FromRedis(message);
			}
			catch (Exception e)
			{
				this.logger.Error(0, "GetQueueMessagesAsync", e);
				throw e;
			}
			this.logger.Verbose("GetQueueMessagesAsync OK.");
			//return Enumerable.ToList<IBatchContainer>(new IBatchContainer[] { batchContainer });
			return new List<IBatchContainer>() { batchContainer };
		}

		public Task MessagesDeliveredAsync(IList<IBatchContainer> messages)
		{
			// No aplica
			return TaskDone.Done;
		}

		public Task Shutdown(TimeSpan timeout)
		{
			return TaskDone.Done;
		}
	}
}
