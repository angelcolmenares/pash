using System;
using System.Configuration;

namespace Microsoft.ActiveDirectory.Management
{
	public class ConfigurationHandler : ConfigurationSection
	{
		private const string TraceElementName = "Trace";

		[ConfigurationProperty("Trace", DefaultValue=null, IsRequired=false)]
		public TraceElement TraceSection
		{
			get
			{
				return (TraceElement)base["Trace"];
			}
			set
			{
				base["Trace"] = value;
			}
		}

		public ConfigurationHandler()
		{
		}
	}
}