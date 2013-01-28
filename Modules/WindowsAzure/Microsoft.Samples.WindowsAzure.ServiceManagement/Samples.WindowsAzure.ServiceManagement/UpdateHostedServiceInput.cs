using System;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[DataContract(Name="UpdateHostedService", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class UpdateHostedServiceInput : IExtensibleDataObject
	{
		[DataMember(Order=4, EmitDefaultValue=false)]
		public string AffinityGroup
		{
			get;
			set;
		}

		[DataMember(Order=2, EmitDefaultValue=false)]
		public string Description
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

		[DataMember(Order=1, EmitDefaultValue=false)]
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

		public UpdateHostedServiceInput()
		{
		}
	}
}