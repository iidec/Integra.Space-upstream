using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integra.Space.StreamProvider.Redis
{
	public class RedisConnectionString
	{
		string DatabaseIdParameterKey = "DatabaseId";
		string HostAddressParameterKey = "HostAddress";
		string HostPortParameterKey = "HostPort";
		string PasswordParameterKey = "Password";
		NameValueCollection parameters;

		public RedisConnectionString(string connectionString)
		{
			this.parameters = RedisConnectionStringParser.Parse(connectionString, "(DatabaseId|HostAddress|HostPort|Password)", @"([^\s]+)");
		}

		public void Validate()
		{
			if (this.parameters == null)
				throw new ArgumentNullException(nameof(parameters));

			int databaseId;

			if (!int.TryParse(this.parameters[DatabaseIdParameterKey], out databaseId))
				throw new ArgumentNullException(nameof(DatabaseIdParameterKey));

			if (this.parameters[this.HostAddressParameterKey] == null)
				throw new ArgumentNullException(nameof(HostAddressParameterKey));

			if (this.parameters[HostPortParameterKey] == null)
				throw new ArgumentNullException(nameof(HostPortParameterKey));

			int hostPort;

			if (!int.TryParse(this.parameters[HostPortParameterKey], out hostPort))
				throw new ArgumentNullException(nameof(HostPortParameterKey));

			if (this.parameters[PasswordParameterKey] == null)
				throw new ArgumentNullException(nameof(PasswordParameterKey));

			this.DatabaseId = databaseId;
			this.HostAddress = this.parameters[this.HostAddressParameterKey];
			this.HostPort = hostPort;
			this.Password = this.parameters[this.PasswordParameterKey];
		}

		public int DatabaseId
		{
			get;
			private set;
		}

		public string HostAddress
		{
			get;
			private set;
		}

		public int HostPort
		{
			get;
			private set;
		}

		public string Password
		{
			get;
			private set;
		}
	}
}
