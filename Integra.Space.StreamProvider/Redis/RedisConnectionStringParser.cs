using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Integra.Space.StreamProvider.Redis
{
	internal static class RedisConnectionStringParser
	{
		public static NameValueCollection Parse(string connectionString, string keysPattern, string valuesPattern)
		{
			Regex valuePatternRegex = new Regex(valuesPattern);
			Regex keyPatternRegex = new Regex(keysPattern, RegexOptions.IgnoreCase);

			NameValueCollection values = new NameValueCollection(System.StringComparer.InvariantCultureIgnoreCase);

			if (!string.IsNullOrWhiteSpace(connectionString))
			{
				string[] splitResult = Regex.Split(";" + connectionString, string.Concat(";", keysPattern, "="), RegexOptions.IgnoreCase);

				if (splitResult.Length <= 0)
				{
					return values;
				}

				if (!string.IsNullOrWhiteSpace(splitResult[0]))
				{
					//TODO: Cambiar
					throw new Exception();//ConfigurationErrorsException(SRClient.AppSettingsConfigSettingInvalidKey(connectionString));
				}

				if ((splitResult.Length % 2) != 1)
				{
					//TODO: Cambiar
					throw new Exception();//ConfigurationErrorsException(SRClient.AppSettingsConfigSettingInvalidKey(connectionString));
				}

				for (int i = 1; i < splitResult.Length; i++)
				{
					string str2 = splitResult[i];
					if (string.IsNullOrWhiteSpace(str2) || !keyPatternRegex.IsMatch(str2))
					{
						//TODO: Cambiar
						throw new Exception();//ConfigurationErrorsException(SRClient.AppSettingsConfigSettingInvalidKey(str2));
					}
					string str3 = splitResult[i + 1];
					if (string.IsNullOrWhiteSpace(str3) || !valuePatternRegex.IsMatch(str3))
					{
						//TODO: Cambiar
						throw new Exception();//ConfigurationErrorsException(SRClient.AppSettingsConfigSettingInvalidValue(str2, str3));
					}
					if (values[str2] != null)
					{
						//TODO: Cambiar
						throw new Exception();//ConfigurationErrorsException(SRClient.AppSettingsConfigDuplicateSetting(str2));
					}
					values[str2] = str3;
					i++;
				}
			}

			return values;
		}
	}
}
