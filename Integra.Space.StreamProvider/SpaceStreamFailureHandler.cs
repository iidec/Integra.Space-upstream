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
	class SpaceStreamFailureHandler : IStreamFailureHandler
	{
		private readonly Logger logger;

		public SpaceStreamFailureHandler(Logger logger)
		{
			this.logger = logger;
		}

		public bool ShouldFaultSubsriptionOnError { get; } = true;

		public Task OnDeliveryFailure(GuidId subscriptionId, string streamProviderName, IStreamIdentity streamIdentity, StreamSequenceToken sequenceToken)
		{
			logger.Error(0, $"provider name: {streamProviderName}, sub id: {subscriptionId}, stream id: {streamIdentity}, token: {sequenceToken}");
			return TaskDone.Done;
		}

		public Task OnSubscriptionFailure(GuidId subscriptionId, string streamProviderName, IStreamIdentity streamIdentity, StreamSequenceToken sequenceToken)
		{
			logger.Error(0, $"provider name: {streamProviderName}, sub id: {subscriptionId}, stream id: {streamIdentity}, token: {sequenceToken}");
			return TaskDone.Done;
		}
	}
}
