using Newtonsoft.Json;
using Orleans.Serialization;
using Orleans.Streams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integra.Space.StreamProvider
{
	public class BinaryBatchContainerSerializer<T> : IBatchContainerSerializer where T : IBatchContainer
	{
		public IBatchContainer Deserialize(Stream stream)
		{
			var length = stream.Length;
			byte[] buffer = new byte[length];
			stream.Read(buffer, 0, (int)length);
			return SerializationManager.DeserializeFromByteArray<IBatchContainer>(buffer);
		}

		public void Serialize(Stream stream, IBatchContainer batchContainer)
		{
			var buffer = SerializationManager.SerializeToByteArray(batchContainer);
			stream.Write(buffer, 0, buffer.Length);
		}
	}
}
