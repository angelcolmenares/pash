using System;
using System.Configuration;

namespace Microsoft.WSMan
{
	public class ManagementConfigurationElementCollection : ConfigurationElementCollection
	{
		public ManagementConfigurationElementCollection ()
		{

		}

		#region implemented abstract members of ConfigurationElementCollection
		protected override ConfigurationElement CreateNewElement ()
		{
			return new ManagementConfigurationElement();
		}

		protected override object GetElementKey (ConfigurationElement element)
		{
			ManagementConfigurationElement target = element as ManagementConfigurationElement;
			if (target == null) return null;
			return target.ResourceUri;
		}
		#endregion
	}
}

