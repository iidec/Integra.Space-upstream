using Integra.Space.StreamProvider.Redis;
using Orleans.Providers;
using Orleans.Providers.Streams.Common;
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
	/// Adapter factory.  This should create an adapter from the stream provider configuration
	/// </summary>

	public class SpaceAdapterFactory : IQueueAdapterFactory
	{
		private string providerName;
		private Logger logger;
		private RedisConnectionString connectionString;
		private HashRingBasedStreamQueueMapper queueMapper;
		private IQueueAdapterCache adapterCache;

		/// <summary>
		/// Initialize the factory.
		/// </summary>
		/// <param name="config"></param>
		/// <param name="providerName"></param>
		/// <param name="logger"></param>
		/// <param name="serviceProvider"></param>
		public void Init(IProviderConfiguration config, string providerName, Logger logger, IServiceProvider serviceProvider)
		{
			if (config == null) throw new ArgumentNullException(nameof(config));
			if (providerName == null) throw new ArgumentNullException(nameof(providerName));
			if (logger == null) throw new ArgumentNullException(nameof(logger));
			if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));

			this.providerName = providerName;
			this.logger = logger;
			this.connectionString = new RedisConnectionString(config.Properties["ConnectionString"]);
			connectionString.Validate();
			this.queueMapper = new HashRingBasedStreamQueueMapper(1, this.providerName);
			this.adapterCache = new SimpleQueueAdapterCache(1024, this.logger);
		}

		public Task<IQueueAdapter> CreateAdapter()
		{
			//var adapter = new SpaceQueueAdapter(this.queueMapper, this.providerName, this.connectionString, this.logger, new RedisBatchContainerFactory(new JsonBatchContainerSerializer<RedisBatchContainer>()));
			var adapter = new SpaceQueueAdapter(this.queueMapper, this.providerName, this.connectionString, this.logger, new RedisBatchContainerFactory(new BinaryBatchContainerSerializer<RedisBatchContainer>()));
			return Task.FromResult<IQueueAdapter>(adapter);
		}

		public Task<IStreamFailureHandler> GetDeliveryFailureHandler(QueueId queueId)
		{
			return Task.FromResult<IStreamFailureHandler>(new SpaceStreamFailureHandler(this.logger));
		}

		public IQueueAdapterCache GetQueueAdapterCache()
		{
			return this.adapterCache;
		}

		public IStreamQueueMapper GetStreamQueueMapper()
		{
			return this.queueMapper;
		}
	}
}
