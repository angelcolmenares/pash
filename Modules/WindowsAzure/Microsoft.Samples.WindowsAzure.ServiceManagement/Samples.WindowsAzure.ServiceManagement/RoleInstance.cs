using System;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class RoleInstance : IExtensibleDataObject
	{
		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		[DataMember(Order=13, EmitDefaultValue=false)]
		public string HostName
		{
			get;
			set;
		}

		[DataMember(Order=11, EmitDefaultValue=false)]
		public InstanceEndpointList InstanceEndpoints
		{
			get;
			set;
		}

		[DataMember(Order=8, EmitDefaultValue=false)]
		public string InstanceErrorCode
		{
			get;
			set;
		}

		[DataMember(Order=5, EmitDefaultValue=false)]
		public int? InstanceFaultDomain
		{
			get;
			set;
		}

		[DataMember(Order=2)]
		public string InstanceName
		{
			get;
			set;
		}

		[DataMember(Order=6, EmitDefaultValue=false)]
		public string InstanceSize
		{
			get;
			set;
		}

		[DataMember(Order=7, EmitDefaultValue=false)]
		public string InstanceStateDetails
		{
			get;
			set;
		}

		[DataMember(Order=3)]
		public string InstanceStatus
		{
			get;
			set;
		}

		[DataMember(Order=4, EmitDefaultValue=false)]
		public int? InstanceUpgradeDomain
		{
			get;
			set;
		}

		[DataMember(Order=10, EmitDefaultValue=false)]
		public string IpAddress
		{
			get;
			set;
		}

		[DataMember(Order=12, EmitDefaultValue=false)]
		public string PowerState
		{
			get;
			set;
		}

		[DataMember(Order=1)]
		public string RoleName
		{
			get;
			set;
		}

		public RoleInstance()
		{
		}
	}
}