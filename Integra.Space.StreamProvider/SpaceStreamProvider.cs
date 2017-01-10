using Orleans.Providers.Streams.Common;
using Orleans.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integra.Space.StreamProvider
{
	public class SpaceStreamProvider : PersistentStreamProvider<SpaceAdapterFactory>, IStreamProvider
	{
	}
}
