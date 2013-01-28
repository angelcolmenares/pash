using System;
using System.Configuration;

namespace Microsoft.WSMan
{
	public class EventingConfigurationElementCollection : ConfigurationElementCollection
	{
		public EventingConfigurationElementCollection ()
		{

		}

		#region implemented abstract members of ConfigurationElementCollection

		protected override ConfigurationElement CreateNewElement ()
		{
			return new EventingConfigurationElement();
		}

		protected override object GetElementKey (ConfigurationElement element)
		{
			EventingConfigurationElement target = element as EventingConfigurationElement;
			if (target == null) return null;
			return target.ResourceUri;
		}

		#endregion
	}
}

