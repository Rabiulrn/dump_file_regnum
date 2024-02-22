using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegnumServices
{
	public class ConfigSettings
	{
		public string configSetting(string ConnectionStringName)
		{
			var configsetting = new ConfigurationBuilder().
			AddJsonFile("appsettings.json").Build();
			return configsetting[$@"ConnectionStrings:{ConnectionStringName}"].ToString();
		
		}
		
		public string QuerySetting(string valueName)
		{
			var configsetting = new ConfigurationBuilder().
			AddJsonFile("appsettings.json").Build();
			return configsetting[$@"DB_Query_Values:{valueName}"].ToString();
		
		}
		public string TimersSetting(string timeInterval)
		{
			var configsetting = new ConfigurationBuilder().
			AddJsonFile("appsettings.json").Build();
			return configsetting[$@"LoggingTimes:{timeInterval}"].ToString();
		
		}
		
	}
}
