using System;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class CreateStorageServiceInput : IExtensibleDataObject
	{
		[DataMember(Order=4, EmitDefaultValue=false)]
		public string AffinityGroup
		{
			get;
			set;
		}

		[DataMember(Order=2)]
		public string Description
		{
			get;
			set;
		}

		[DataMember(Order=7, EmitDefaultValue=false)]
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

		[DataMember(Order=6, EmitDefaultValue=false)]
		public bool? GeoReplicationEnabled
		{
			get;
			set;
		}

		[DataMember(Order=3)]
		public string Label
		{
			get;
			set;
		}

		[DataMember(Order=5, EmitDefaultValue=false)]
		public string Location
		{
			get;
			set;
		}

		[DataMember(Order=1, EmitDefaultValue=false)]
		public string ServiceName
		{
			get;
			set;
		}

		public CreateStorageServiceInput()
		{
		}
	}
}