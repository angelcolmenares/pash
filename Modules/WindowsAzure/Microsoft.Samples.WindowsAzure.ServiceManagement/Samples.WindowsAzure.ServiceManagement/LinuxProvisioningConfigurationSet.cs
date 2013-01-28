using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class LinuxProvisioningConfigurationSet : ProvisioningConfigurationSet
	{
		public override string ConfigurationSetType
		{
			get
			{
				return "LinuxProvisioningConfiguration";
			}
			set
			{
				base.ConfigurationSetType = value;
			}
		}

		[DataMember(Name="DisableSshPasswordAuthentication", EmitDefaultValue=false, Order=4)]
		public bool? DisableSshPasswordAuthentication
		{
			get
			{
				return base.GetValue<bool?>("DisableSshPasswordAuthentication");
			}
			set
			{
				base.SetValue<bool?>("DisableSshPasswordAuthentication", value);
			}
		}

		[DataMember(Name="HostName", EmitDefaultValue=false, Order=1)]
		public string HostName
		{
			get
			{
				return base.GetValue<string>("HostName");
			}
			set
			{
				base.SetValue<string>("HostName", value);
			}
		}

		[DataMember(Name="SSH", EmitDefaultValue=false, Order=5)]
		public LinuxProvisioningConfigurationSet.SSHSettings SSH
		{
			get
			{
				return base.GetValue<LinuxProvisioningConfigurationSet.SSHSettings>("SSH");
			}
			set
			{
				base.SetValue<LinuxProvisioningConfigurationSet.SSHSettings>("SSH", value);
			}
		}

		[DataMember(Name="UserName", EmitDefaultValue=false, Order=2)]
		public string UserName
		{
			get
			{
				return base.GetValue<string>("UserName");
			}
			set
			{
				base.SetValue<string>("UserName", value);
			}
		}

		[DataMember(Name="UserPassword", EmitDefaultValue=false, Order=3)]
		public string UserPassword
		{
			get
			{
				return base.GetValue<string>("UserPassword");
			}
			set
			{
				base.SetValue<string>("UserPassword", value);
			}
		}

		public LinuxProvisioningConfigurationSet()
		{
		}

		[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
		public class SSHKeyPair
		{
			[DataMember(Name="Fingerprint", EmitDefaultValue=false, Order=1)]
			public string Fingerprint
			{
				get;
				set;
			}

			[DataMember(Name="Path", EmitDefaultValue=false, Order=2)]
			public string Path
			{
				get;
				set;
			}

			public SSHKeyPair()
			{
			}
		}

		[CollectionDataContract(Name="SSHKeyPairList", ItemName="KeyPair", Namespace="http://schemas.microsoft.com/windowsazure")]
		public class SSHKeyPairList : List<LinuxProvisioningConfigurationSet.SSHKeyPair>
		{
			public SSHKeyPairList()
			{
			}
		}

		[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
		public class SSHPublicKey
		{
			[DataMember(Name="Fingerprint", EmitDefaultValue=false, Order=1)]
			public string Fingerprint
			{
				get;
				set;
			}

			[DataMember(Name="Path", EmitDefaultValue=false, Order=2)]
			public string Path
			{
				get;
				set;
			}

			public SSHPublicKey()
			{
			}
		}

		[CollectionDataContract(Name="SSHPublicKeyList", ItemName="PublicKey", Namespace="http://schemas.microsoft.com/windowsazure")]
		public class SSHPublicKeyList : List<LinuxProvisioningConfigurationSet.SSHPublicKey>
		{
			public SSHPublicKeyList()
			{
			}
		}

		[DataContract(Name="SSHSettings", Namespace="http://schemas.microsoft.com/windowsazure")]
		public class SSHSettings
		{
			[DataMember(Name="KeyPairs", EmitDefaultValue=false, Order=2)]
			public LinuxProvisioningConfigurationSet.SSHKeyPairList KeyPairs
			{
				get;
				set;
			}

			[DataMember(Name="PublicKeys", EmitDefaultValue=false, Order=1)]
			public LinuxProvisioningConfigurationSet.SSHPublicKeyList PublicKeys
			{
				get;
				set;
			}

			public SSHSettings()
			{
			}
		}
	}
}