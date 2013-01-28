using System;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class Deployment : IExtensibleDataObject
	{
		[DataMember(Order=7, EmitDefaultValue=false)]
		public string Configuration
		{
			get;
			set;
		}

		[DataMember(Order=17, EmitDefaultValue=false)]
		public string CreatedTime
		{
			get;
			set;
		}

		[DataMember(Order=2, EmitDefaultValue=false)]
		public string DeploymentSlot
		{
			get;
			set;
		}

		[DataMember(Order=20, EmitDefaultValue=false)]
		public DnsSettings Dns
		{
			get;
			set;
		}

		[DataMember(Order=19, EmitDefaultValue=false)]
		public ExtendedPropertiesList ExtendedProperties
		{
			get;
			set;
		}

		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		[DataMember(Order=5, EmitDefaultValue=false)]
		public string Label
		{
			get;
			set;
		}

		[DataMember(Order=18, EmitDefaultValue=false)]
		public string LastModifiedTime
		{
			get;
			set;
		}

		[DataMember(Order=14, EmitDefaultValue=false)]
		public bool? Locked
		{
			get;
			set;
		}

		[DataMember(Order=1, EmitDefaultValue=false)]
		public string Name
		{
			get;
			set;
		}

		[DataMember(Order=21, EmitDefaultValue=false)]
		public PersistentVMDowntimeInfo PersistentVMDowntime
		{
			get;
			set;
		}

		[DataMember(Order=3, EmitDefaultValue=false)]
		public string PrivateID
		{
			get;
			set;
		}

		[DataMember(Order=8, EmitDefaultValue=false)]
		public RoleInstanceList RoleInstanceList
		{
			get;
			set;
		}

		[DataMember(Order=12, EmitDefaultValue=false)]
		public RoleList RoleList
		{
			get;
			set;
		}

		[DataMember(Order=15, EmitDefaultValue=false)]
		public bool? RollbackAllowed
		{
			get;
			set;
		}

		[DataMember(Order=13, EmitDefaultValue=false)]
		public string SdkVersion
		{
			get;
			set;
		}

		[DataMember(Order=4, EmitDefaultValue=false)]
		public string Status
		{
			get;
			set;
		}

		[DataMember(Order=11, EmitDefaultValue=false)]
		public int UpgradeDomainCount
		{
			get;
			set;
		}

		[DataMember(Order=10, EmitDefaultValue=false)]
		public UpgradeStatus UpgradeStatus
		{
			get;
			set;
		}

		[DataMember(Order=6, EmitDefaultValue=false)]
		public Uri Url
		{
			get;
			set;
		}

		[DataMember(Order=16, EmitDefaultValue=false)]
		public string VirtualNetworkName
		{
			get;
			set;
		}

		public Deployment()
		{
		}
	}
}