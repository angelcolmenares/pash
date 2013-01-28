using System;
using System.Configuration;

namespace Microsoft.WSMan
{
	public class EventingConfigurationElement : ConfigurationElement
	{
		public EventingConfigurationElement ()
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

		[ConfigurationProperty("filterType", IsRequired = true)]
		public string FilterType {
			get { return (string)base ["filterType"]; }
			set { base ["filterType"] = value; }
		}

		[ConfigurationProperty("deliveryUri", IsRequired = true)]
		public string DeliveryUri {
			get { return (string)base ["deliveryUri"]; }
			set { base ["deliveryUri"] = value; }
		}

		[ConfigurationProperty("dialect", IsRequired = true)]
		public string Dialect {
			get { return (string)base ["dialect"]; }
			set { base ["dialect"] = value; }
		}
	}
}

