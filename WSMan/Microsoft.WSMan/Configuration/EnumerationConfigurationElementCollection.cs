using System;
using System.Configuration;

namespace Microsoft.WSMan
{
	public class EnumerationConfigurationElementCollection : ConfigurationElementCollection
	{
		public EnumerationConfigurationElementCollection ()
		{

		}

		#region implemented abstract members of ConfigurationElementCollection

		protected override ConfigurationElement CreateNewElement ()
		{
			return new EnumerationConfigurationElement();
		}

		protected override object GetElementKey (ConfigurationElement element)
		{
			EnumerationConfigurationElement target = element as EnumerationConfigurationElement;
			if (target == null) return null;
			return target.ResourceUri;
		}

		#endregion
	}
}

