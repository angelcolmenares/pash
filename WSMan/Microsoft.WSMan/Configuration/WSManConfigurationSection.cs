using System;
using System.Configuration;

namespace Microsoft.WSMan.Configuration
{
	public class WSManConfigurationSection : ConfigurationSection
	{
		public const string SectionName = "wsMan";

		[ConfigurationProperty("management")]
		public ManagementConfigurationElementCollection ManagementHandlers
		{
			get { return (ManagementConfigurationElementCollection)base["management"]; }
			set { base["management"] = value; }
		}

		[ConfigurationProperty("eventing")]
		public EventingConfigurationElementCollection EventHandlers
		{
			get { return (EventingConfigurationElementCollection)base["eventing"]; }
			set { base["eventing"] = value; }
		}

		[ConfigurationProperty("enumeration")]
		public EnumerationConfigurationElementCollection EnumerationHandlers
		{
			get { return (EnumerationConfigurationElementCollection)base["enumeration"]; }
			set { base["enumeration"] = value; }
		}

	}
}

