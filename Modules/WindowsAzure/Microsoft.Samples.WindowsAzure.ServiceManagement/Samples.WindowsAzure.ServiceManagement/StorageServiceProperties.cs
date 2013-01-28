using System;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class StorageServiceProperties : IExtensibleDataObject
	{
		[DataMember(Order=2, EmitDefaultValue=false)]
		public string AffinityGroup
		{
			get;
			set;
		}

		[DataMember(Order=1)]
		public string Description
		{
			get;
			set;
		}

		[DataMember(Order=6, EmitDefaultValue=false)]
		public EndpointList Endpoints
		{
			get;
			set;
		}

		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		[DataMember(Order=8, EmitDefaultValue=false)]
		public string GeoPrimaryRegion
		{
			get;
			set;
		}

		[DataMember(Order=7, EmitDefaultValue=false)]
		public bool? GeoReplicationEnabled
		{
			get;
			set;
		}

		[DataMember(Order=10, EmitDefaultValue=false)]
		public string GeoSecondaryRegion
		{
			get;
			set;
		}

		[DataMember(Order=4, EmitDefaultValue=false)]
		public string Label
		{
			get;
			set;
		}

		[DataMember(Order=12, EmitDefaultValue=false)]
		public string LastGeoFailoverTime
		{
			get;
			set;
		}

		[DataMember(Order=3, EmitDefaultValue=false)]
		public string Location
		{
			get;
			set;
		}

		[DataMember(Order=5, EmitDefaultValue=false)]
		public string Status
		{
			get;
			set;
		}

		[DataMember(Order=9, EmitDefaultValue=false)]
		public string StatusOfPrimary
		{
			get;
			set;
		}

		[DataMember(Order=11, EmitDefaultValue=false)]
		public string StatusOfSecondary
		{
			get;
			set;
		}

		public StorageServiceProperties()
		{
		}
	}
}