using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integra.Space.StreamProvider.TestGrainInterfaces
{
	public interface IBasicStreaming_ProducerGrain : IGrainWithGuidKey
	{
		Task BecomeProducer(Guid streamId, string streamNamespace, string providerToUse);

		Task StartPeriodicProducing();

		Task StopPeriodicProducing();

		Task<int> GetNumberProduced();

		Task ClearNumberProduced();
		Task Produce();
	}

	public interface IBasicStreaming_ConsumerGrain : IConsumerGrain
	{
		Task BecomeConsumer(Guid streamId, string streamNamespace, string providerToUse);

		Task StopConsuming();
	}

	public interface IConsumerGrain : IGrainWithGuidKey
	{
		Task<int> GetNumberConsumed();
	}
}
