using System;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class AffinityGroup : IExtensibleDataObject
	{
		[DataMember(Order=7, EmitDefaultValue=false)]
		public CapabilitiesList Capabilities
		{
			get;
			set;
		}

		[DataMember(Order=3)]
		public string Description
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
		public HostedServiceList HostedServices
		{
			get;
			set;
		}

		[DataMember(Order=2)]
		public string Label
		{
			get;
			set;
		}

		[DataMember(Order=4)]
		public string Location
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

		[DataMember(Order=6, EmitDefaultValue=false)]
		public StorageServiceList StorageServices
		{
			get;
			set;
		}

		public AffinityGroup()
		{
		}
	}
}