using System;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class UpdateStorageServiceInput : IExtensibleDataObject
	{
		[DataMember(Order=1)]
		public string Description
		{
			get;
			set;
		}

		[DataMember(Order=4, EmitDefaultValue=false)]
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

		[DataMember(Order=3, EmitDefaultValue=false)]
		public bool? GeoReplicationEnabled
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

		public UpdateStorageServiceInput()
		{
		}
	}
}