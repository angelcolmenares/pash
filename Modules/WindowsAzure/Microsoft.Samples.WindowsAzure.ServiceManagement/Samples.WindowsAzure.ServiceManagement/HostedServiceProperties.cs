using System;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class HostedServiceProperties : IExtensibleDataObject
	{
		[DataMember(Order=2, EmitDefaultValue=false)]
		public string AffinityGroup
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=6)]
		public string DateCreated
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=7)]
		public string DateLastModified
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

		[DataMember(EmitDefaultValue=false, Order=8)]
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

		[DataMember(Order=4)]
		public string Label
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

		[DataMember(EmitDefaultValue=false, Order=5)]
		public string Status
		{
			get;
			set;
		}

		public HostedServiceProperties()
		{
		}
	}
}