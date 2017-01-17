using Orleans.Providers.Streams.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integra.Space.StreamProvider.Redis
{
	[Serializable]
	public class RedisEventSequenceToken : EventSequenceToken
	{
		// TODO: Revisar este constructor se usa porque la deserializacion en JSON da problemas
		//public RedisEventSequenceToken() : base(0)
		//{
		//}

		public RedisEventSequenceToken(long seqNumber) : base(seqNumber)
		{
		}

		public RedisEventSequenceToken(long seqNumber, int eventInd) : base(seqNumber, eventInd)
		{
		}
	}
}
