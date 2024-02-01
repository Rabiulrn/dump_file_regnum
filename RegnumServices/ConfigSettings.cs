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
		
	}
}
