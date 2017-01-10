using Orleans.Streams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integra.Space.StreamProvider.Redis
{
	public class RedisBatchContainerFactory : IRedisBatchContainerFactory
	{
		IBatchContainerSerializer serializer;

		public RedisBatchContainerFactory(IBatchContainerSerializer serializer)
		{
			this.serializer = serializer;
		}

		public RedisMessage ToRedisMessage<T>(Guid streamId, string streamNamespace, IEnumerable<T> events, RedisEventSequenceToken sequenceToken, Dictionary<string, object> requestContext)
		{
			RedisBatchContainer batchContainer = new RedisBatchContainer(streamId, streamNamespace, sequenceToken, events.Cast<object>().ToList(), requestContext);
			MemoryStream ms = new MemoryStream();
			serializer.Serialize(ms, batchContainer);
			return new RedisMessage(ms.ToArray());
		}

		public IBatchContainer FromRedis(RedisMessage message)
		{
			using (MemoryStream ms = new MemoryStream(message.Value))
			{
				ms.Position = 0;
				return serializer.Deserialize(ms);
			}
		}
	}
}
