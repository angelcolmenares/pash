using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	[KnownType(typeof(PersistentVMRole))]
	public class Role : Mergable<PersistentVMRole>, IExtensibleDataObject
	{
		[DataMember(Name="ConfigurationSets", EmitDefaultValue=false, Order=4)]
		public Collection<ConfigurationSet> ConfigurationSets
		{
			get
			{
				return base.GetValue<Collection<ConfigurationSet>>("ConfigurationSets");
			}
			set
			{
				base.SetValue<Collection<ConfigurationSet>>("ConfigurationSets", value);
			}
		}

		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		public NetworkConfigurationSet NetworkConfigurationSet
		{
			get
			{
				if (this.ConfigurationSets != null)
				{
					Collection<ConfigurationSet> configurationSets = this.ConfigurationSets;
					return configurationSets.FirstOrDefault<ConfigurationSet>((ConfigurationSet cset) => cset is NetworkConfigurationSet) as NetworkConfigurationSet;
				}
				else
				{
					return null;
				}
			}
			set
			{
				if (this.ConfigurationSets == null)
				{
					this.ConfigurationSets = new Collection<ConfigurationSet>();
				}
				Collection<ConfigurationSet> configurationSets = this.ConfigurationSets;
				NetworkConfigurationSet networkConfigurationSet = configurationSets.FirstOrDefault<ConfigurationSet>((ConfigurationSet cset) => cset is NetworkConfigurationSet) as NetworkConfigurationSet;
				if (networkConfigurationSet != null)
				{
					this.ConfigurationSets.Remove(networkConfigurationSet);
				}
				this.ConfigurationSets.Add(value);
			}
		}

		[DataMember(Order=2)]
		public string OsVersion
		{
			get;
			set;
		}

		[DataMember(Order=1)]
		public virtual string RoleName
		{
			get;
			set;
		}

		[DataMember(Order=3, EmitDefaultValue=false)]
		public virtual string RoleType
		{
			get;
			set;
		}

		public Role()
		{
		}

		public override object ResolveType()
		{
			if (base.GetType() == typeof(Role))
			{
				if (this.RoleType != typeof(PersistentVMRole).Name)
				{
					return this;
				}
				else
				{
					return base.Convert<PersistentVMRole>();
				}
			}
			else
			{
				return this;
			}
		}
	}
}