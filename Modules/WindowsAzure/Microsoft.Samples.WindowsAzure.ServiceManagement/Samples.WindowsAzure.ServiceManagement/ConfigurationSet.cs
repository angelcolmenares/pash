using System;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	[KnownType(typeof(NetworkConfigurationSet))]
	[KnownType(typeof(ProvisioningConfigurationSet))]
	[KnownType(typeof(WindowsProvisioningConfigurationSet))]
	[KnownType(typeof(LinuxProvisioningConfigurationSet))]
	public class ConfigurationSet : Mergable<ConfigurationSet>
	{
		[DataMember(EmitDefaultValue=false, Order=0)]
		public virtual string ConfigurationSetType
		{
			get;
			set;
		}

		protected ConfigurationSet()
		{
		}

		public override object ResolveType()
		{
			if (base.GetType() == typeof(ConfigurationSet))
			{
				if (!string.IsNullOrEmpty(this.ConfigurationSetType))
				{
					if (!string.Equals(this.ConfigurationSetType, "WindowsProvisioningConfiguration"))
					{
						if (!string.Equals(this.ConfigurationSetType, "LinuxProvisioningConfiguration"))
						{
							if (string.Equals(this.ConfigurationSetType, "NetworkConfiguration"))
							{
								return base.Convert<NetworkConfigurationSet>();
							}
						}
						else
						{
							return base.Convert<LinuxProvisioningConfigurationSet>();
						}
					}
					else
					{
						return base.Convert<WindowsProvisioningConfigurationSet>();
					}
				}
				return this;
			}
			else
			{
				return this;
			}
		}
	}
}