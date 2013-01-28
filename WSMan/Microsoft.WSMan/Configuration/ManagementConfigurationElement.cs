using System;
using System.Configuration;

namespace Microsoft.WSMan
{
	public class ManagementConfigurationElement : ConfigurationElement
	{
		public ManagementConfigurationElement ()
		{
		}

		
		[ConfigurationProperty("resourceUri", IsRequired = true)]
		public string ResourceUri {
			get { return (string)base ["resourceUri"]; }
			set { base ["resourceUri"] = value; }
		}
		
		[ConfigurationProperty("handlerType", IsRequired = true)]
		public string HandlerType {
			get { return (string)base ["handlerType"]; }
			set { base ["handlerType"] = value; }
		}
	}
}

