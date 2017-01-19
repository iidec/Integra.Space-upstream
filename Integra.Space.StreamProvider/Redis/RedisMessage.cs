using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integra.Space.StreamProvider.Redis
{
	public class RedisMessage
	{
		public RedisMessage(byte[] value)
		{
			this.Value = value;
		}

		public byte[] Value { get; private set; }

		public static readonly RedisMessage Empty = new RedisMessage(null);

		public override bool Equals(object obj)
		{
			// Verifica de mensaje vacio.
			if (obj != null)
			{
				if (obj is RedisMessage)
				{
					if (this.Value == null && ((RedisMessage)obj).Value == null)
					{
						return true;
					}
				}
			}

			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}
