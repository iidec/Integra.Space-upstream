using Orleans.Streams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integra.Space.StreamProvider
{
	public interface IBatchContainerSerializer
	{
		void Serialize(Stream stream, IBatchContainer batchContainer);
		IBatchContainer Deserialize(Stream stream);
	}
}
