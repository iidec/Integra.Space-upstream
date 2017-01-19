using Orleans;
using Orleans.Runtime;
using Orleans.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Integra.Space.StreamProvider.TestGrainInterfaces;

namespace Integra.Space.StreamProvider.TestGrains
{
	internal class SampleConsumerObserver<T> : IAsyncObserver<T>
	{
		private readonly SampleStreaming_ConsumerGrain hostingGrain;

		internal SampleConsumerObserver(SampleStreaming_ConsumerGrain hostingGrain)
		{
			this.hostingGrain = hostingGrain;
		}

		public Task OnNextAsync(T item, StreamSequenceToken token = null)
		{
			try
			{
				hostingGrain.logger.Info("OnNextAsync(item={0}, token={1})", item, token != null ? token.ToString() : "null");
				hostingGrain.numConsumedItems++;
				return TaskDone.Done;
			}
			catch(Exception e)
			{
				var ex = e;
				throw e;
			}
		}

		public Task OnCompletedAsync()
		{
			hostingGrain.logger.Info("OnCompletedAsync()");
			return TaskDone.Done;
		}

		public Task OnErrorAsync(Exception ex)
		{
			hostingGrain.logger.Info("OnErrorAsync({0})", ex);
			return TaskDone.Done;
		}
	}

	public class SampleStreaming_ProducerGrain : Grain, IBasicStreaming_ProducerGrain
	{
		private IAsyncStream<int> producer;
		private int numProducedItems;
		private IDisposable producerTimer;
		internal Logger logger;

		public override Task OnActivateAsync()
		{
			logger = base.GetLogger("SampleStreaming_ProducerGrain " + base.IdentityString);
			logger.Info("OnActivateAsync");
			numProducedItems = 0;
			return TaskDone.Done;
		}

		public Task BecomeProducer(Guid streamId, string streamNamespace, string providerToUse)
		{
			logger.Info("BecomeProducer");
			IStreamProvider streamProvider = base.GetStreamProvider(providerToUse);
			producer = streamProvider.GetStream<int>(streamId, streamNamespace);
			return TaskDone.Done;
		}

		public Task StartPeriodicProducing()
		{
			logger.Info("StartPeriodicProducing");
			producerTimer = base.RegisterTimer(TimerCallback, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(1));
			return TaskDone.Done;
		}

		public Task StopPeriodicProducing()
		{
			logger.Info("StopPeriodicProducing");
			producerTimer.Dispose();
			producerTimer = null;
			return TaskDone.Done;
		}

		public Task<int> GetNumberProduced()
		{
			logger.Info("GetNumberProduced");
			return Task.FromResult(numProducedItems);
		}

		public Task ClearNumberProduced()
		{
			numProducedItems = 0;
			return TaskDone.Done;
		}

		public Task Produce()
		{
			return Fire();
		}

		private Task TimerCallback(object state)
		{
			return producerTimer != null ? Fire() : TaskDone.Done;
		}

		private Task Fire([CallerMemberName] string caller = null)
		{
			numProducedItems++;
			logger.Info("{0} (item={1})", caller, numProducedItems);
			return producer.OnNextAsync(numProducedItems);
		}

		public override Task OnDeactivateAsync()
		{
			logger.Info("OnDeactivateAsync");
			return TaskDone.Done;
		}
	}

	public class SampleStreaming_ConsumerGrain : Grain, IBasicStreaming_ConsumerGrain
	{
		private IAsyncObservable<int> consumer;
		internal int numConsumedItems;
		internal Logger logger;
		private IAsyncObserver<int> consumerObserver;
		private StreamSubscriptionHandle<int> consumerHandle;

		public override Task OnActivateAsync()
		{
			logger = base.GetLogger("SampleStreaming_ConsumerGrain " + base.IdentityString);
			logger.Info("OnActivateAsync");
			numConsumedItems = 0;
			consumerHandle = null;
			return TaskDone.Done;
		}

		public async Task BecomeConsumer(Guid streamId, string streamNamespace, string providerToUse)
		{
			logger.Info("BecomeConsumer");
			consumerObserver = new SampleConsumerObserver<int>(this);
			IStreamProvider streamProvider = base.GetStreamProvider(providerToUse);
			consumer = streamProvider.GetStream<int>(streamId, streamNamespace);
			consumerHandle = await consumer.SubscribeAsync(consumerObserver);
		}

		public async Task StopConsuming()
		{
			logger.Info("StopConsuming");
			if (consumerHandle != null)
			{
				await consumerHandle.UnsubscribeAsync();
				consumerHandle = null;
			}
		}

		public Task<int> GetNumberConsumed()
		{
			return Task.FromResult(numConsumedItems);
		}

		public override Task OnDeactivateAsync()
		{
			logger.Info("OnDeactivateAsync");
			return TaskDone.Done;
		}
	}
}
