using System;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[DataContract(Name="CreateDeployment", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class CreateDeploymentInput : IExtensibleDataObject
	{
		[DataMember(Order=4)]
		public string Configuration
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

		[DataMember(Order=3)]
		public string Label
		{
			get;
			set;
		}

		[DataMember(Order=1)]
		public string Name
		{
			get;
			set;
		}

		[DataMember(Order=2)]
		public Uri PackageUrl
		{
			get;
			set;
		}

		[DataMember(Order=5, EmitDefaultValue=false)]
		public bool? StartDeployment
		{
			get;
			set;
		}

		[DataMember(Order=6, EmitDefaultValue=false)]
		public bool? TreatWarningsAsError
		{
			get;
			set;
		}

		public CreateDeploymentInput()
		{
		}
	}
}