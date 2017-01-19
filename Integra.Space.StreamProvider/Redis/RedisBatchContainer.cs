using Orleans.Runtime;
using Orleans.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integra.Space.StreamProvider.Redis
{
	[Serializable]
	public class RedisBatchContainer : IBatchContainer
	{
		private RedisEventSequenceToken sequenceToken;
		private readonly List<object> events;

		public RedisBatchContainer(Guid streamGuid, string streamNamespace, RedisEventSequenceToken sequenceToken, List<object> events, Dictionary<string, object> requestContext)
		{
			this.StreamGuid = streamGuid;
			this.StreamNamespace = streamNamespace;
			this.sequenceToken = sequenceToken;
			this.events = events;
			this.BatchRequestContext = requestContext;
		}

		public List<object> Events
		{
			get
			{
				return this.events;
			}
		}

		public Dictionary<string, object> BatchRequestContext
		{
			get;
		}

		public StreamSequenceToken SequenceToken
		{
			get
			{
				return this.sequenceToken;
			}
		}

		public Guid StreamGuid
		{
			get;
		}

		public string StreamNamespace
		{
			get;
		}

		public IEnumerable<Tuple<T, StreamSequenceToken>> GetEvents<T>()
		{
			// Get events of the wanted type
			var typedEvents = this.events.OfType<T>();

			// returning the tuple with a unique SequenceToken for each event
			return
				typedEvents.Select(
					(e, i) => Tuple.Create<T, StreamSequenceToken>(e, this.sequenceToken.CreateSequenceTokenForEvent(i)));
		}

		public bool ImportRequestContext()
		{
			if (BatchRequestContext == null)
				return false;

			RequestContext.Import(BatchRequestContext);
			return true;
		}

		public bool ShouldDeliver(IStreamIdentity stream, object filterData, StreamFilterPredicate shouldReceiveFunc)
		{
			foreach (var item in this.events)
			{
				if (shouldReceiveFunc(stream, filterData, item))
					return true; // There is something in this batch that the consumer is interested in, so we should send it.
			}
			return false; // Consumer is not interested in any of these events, so don't send.
		}
	}
}
