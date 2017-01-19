using Orleans.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integra.Space.StreamProvider.Redis
{
	public interface IRedisBatchContainerFactory
	{
		RedisMessage ToRedisMessage<T>(Guid streamId, string streamNamespace, IEnumerable<T> events, RedisEventSequenceToken sequenceToken, Dictionary<string, object> requestContext);
		IBatchContainer FromRedis(RedisMessage message);
	}
}
