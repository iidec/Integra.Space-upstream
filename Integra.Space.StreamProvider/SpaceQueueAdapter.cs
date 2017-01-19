using Integra.Space.StreamProvider.Redis;
using Orleans.Runtime;
using Orleans.Streams;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integra.Space.StreamProvider
{
	/// <summary>
	/// Implement SpaceQueueAdapter class that implements the IQueueAdapter interface,
	/// which is an interfaces that manages access to a sharded queue. IQueueAdapter
	/// manages access to a set of queues/queue partitions
	/// (those are the queues that were returned by IStreamQueueMapper).
	/// It provides an ability to enqueue a message in a specified the queue
	/// and create an IQueueAdapterReceiver for a particular queue.
	/// </summary>
	public class SpaceQueueAdapter : IQueueAdapter
	{
		RedisConnectionString connectionString;
		HashRingBasedStreamQueueMapper streamQueueMapper;
		Logger logger;
		IRedisBatchContainerFactory batchContainerFactory;

		ConcurrentDictionary<QueueId, RedisQueue> queues = new ConcurrentDictionary<QueueId, RedisQueue>();

		public SpaceQueueAdapter(HashRingBasedStreamQueueMapper streamQueueMapper, string providerName, RedisConnectionString connectionString, Logger logger, IRedisBatchContainerFactory batchContainerFactory)
		{
			if (streamQueueMapper == null) throw new ArgumentNullException(nameof(streamQueueMapper));
			if (string.IsNullOrEmpty(providerName)) throw new ArgumentNullException(nameof(providerName));
			if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));
			if (logger == null) throw new ArgumentNullException(nameof(logger));
			if (batchContainerFactory == null) throw new ArgumentNullException(nameof(batchContainerFactory));

			connectionString.Validate(); // Esto puede dar generar un excepcion si esta mal escrito el CN

			this.streamQueueMapper = streamQueueMapper;
			this.Name = providerName;
			this.connectionString = connectionString;
			this.logger = logger;
			this.batchContainerFactory = batchContainerFactory;
		}

		public StreamProviderDirection Direction
		{
			get
			{
				return StreamProviderDirection.ReadWrite;
			}
		}

		public bool IsRewindable
		{
			get
			{
				return false;
			}
		}

		public string Name
		{
			get;
		}

		public IQueueAdapterReceiver CreateReceiver(QueueId queueId)
		{
			this.logger.Verbose("RedisQueueAdapterReceiver - Initialized with HostAddress: {0} HostPort: {1} Password: {2} DatabaseId: {3} QueueId: {4}", connectionString.HostAddress, connectionString.HostPort, connectionString.Password, connectionString.DatabaseId, queueId.ToString());
			RedisQueue queue = new RedisQueue(connectionString.HostAddress, connectionString.HostPort, connectionString.Password, connectionString.DatabaseId, queueId.ToString());
			return new SpaceQueueAdapterReceiver(queue, queueId, this.connectionString, logger, this.batchContainerFactory);
		}

		int id = 0;

		public Task QueueMessageBatchAsync<T>(Guid streamGuid, string streamNamespace, IEnumerable<T> events, StreamSequenceToken token, Dictionary<string, object> requestContext)
		{
			this.logger.Verbose("QueueMessageBatchAsync...");
			QueueId key = streamQueueMapper.GetQueueForStream(streamGuid, streamNamespace);

			RedisQueue queue;

			if (!queues.TryGetValue(key, out queue))
			{
				RedisQueue newQueue = new RedisQueue(connectionString.HostAddress, connectionString.HostPort, connectionString.Password, connectionString.DatabaseId, key.ToString());
				queue = queues.GetOrAdd(key, newQueue);
			}

			try
			{
				RedisMessage message = this.batchContainerFactory.ToRedisMessage<T>(streamGuid, streamNamespace, events, new RedisEventSequenceToken(++id), requestContext);
				this.logger.Verbose("QueueMessageBatchAsync OK");
				return queue.EnqueueAsync(message);
			}
			catch (Exception e)
			{
				this.logger.Error(0, "QueueMessageBatchAsync", e);
				throw e;
			}
		}
	}
}
