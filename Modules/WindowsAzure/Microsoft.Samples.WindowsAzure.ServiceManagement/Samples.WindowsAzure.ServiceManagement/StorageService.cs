using System;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class StorageService : IExtensibleDataObject
	{
		[DataMember(Order=6, EmitDefaultValue=false)]
		public CapabilitiesList Capabilities
		{
			get;
			set;
		}

		[DataMember(Order=5, EmitDefaultValue=false)]
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

		[DataMember(Order=2, EmitDefaultValue=false)]
		public string ServiceName
		{
			get;
			set;
		}

		[DataMember(Order=4, EmitDefaultValue=false)]
		public StorageServiceKeys StorageServiceKeys
		{
			get;
			set;
		}

		[DataMember(Order=3, EmitDefaultValue=false)]
		public StorageServiceProperties StorageServiceProperties
		{
			get;
			set;
		}

		[DataMember(Order=1)]
		public Uri Url
		{
			get;
			set;
		}

		public StorageService()
		{
		}
	}
}