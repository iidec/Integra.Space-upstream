using Newtonsoft.Json;
using Orleans.Streams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integra.Space.StreamProvider
{
	public class JsonBatchContainerSerializer<T> : IBatchContainerSerializer where T : IBatchContainer
	{
		public IBatchContainer Deserialize(Stream stream)
		{
			//var length = stream.Length;
			//byte[] buffer = new byte[length];
			//stream.Read(buffer, 0, (int)length);
			//return SerializationManager.DeserializeFromByteArray<IBatchContainer>(buffer);

			using (StreamReader reader = new StreamReader(stream))
			using (JsonTextReader jsonReader = new JsonTextReader(reader))
			{
				JsonSerializer ser = new JsonSerializer();
				return ser.Deserialize<T>(jsonReader);
			}
		}

		public void Serialize(Stream stream, IBatchContainer batchContainer)
		{
			using (StreamWriter writer = new StreamWriter(stream))
			using (JsonTextWriter jsonWriter = new JsonTextWriter(writer))
			{
				JsonSerializer ser = new JsonSerializer();
				ser.Serialize(jsonWriter, batchContainer);
				jsonWriter.Flush();
			}
		}
	}
}
