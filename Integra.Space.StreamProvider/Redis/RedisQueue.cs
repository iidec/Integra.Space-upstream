using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integra.Space.StreamProvider.Redis
{
	public class RedisQueue : IDisposable
	{
		string queueName;
		private static ConnectionMultiplexer redisConnection;
		IDatabase database;

		public RedisQueue(string HostAddress, int hostPort, string password, int databaseId, string queueName)
		{
			ConfigurationOptions config = new ConfigurationOptions()
			{
				EndPoints =
				{
					{ HostAddress, hostPort }
				},
				Password = password
			};

			this.queueName = queueName;
			lock (typeof(RedisQueue))
			{
				if (RedisQueue.redisConnection == null)
				{
					RedisQueue.redisConnection = ConnectionMultiplexer.Connect(config);
				}
			}
			this.database = RedisQueue.redisConnection.GetDatabase(databaseId);
		}

		public virtual Task EnqueueAsync(RedisMessage message)
		{
			try
			{
				return this.database.ListLeftPushAsync(this.queueName, message.Value);
			}
			catch (Exception e)
			{
				throw e;
			}
		}

		public async virtual Task<RedisMessage> DequeueAsync()
		{
			try
			{
				RedisValue poppedValue = await this.database.ListRightPopAsync(this.queueName);

				if (poppedValue.HasValue)
				{
					return new RedisMessage((byte[])poppedValue);
				}
				else
				{
					return RedisMessage.Empty;
				}
			}
			catch (Exception e)
			{
				throw e;
			}
		}

		public void Dispose()
		{
			RedisQueue.redisConnection.Close();
		}
	}
}
