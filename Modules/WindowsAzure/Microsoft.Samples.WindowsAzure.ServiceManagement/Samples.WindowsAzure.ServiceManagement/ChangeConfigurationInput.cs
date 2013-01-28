using System;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[DataContract(Name="ChangeConfiguration", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class ChangeConfigurationInput : IExtensibleDataObject
	{
		[DataMember(Order=1)]
		public string Configuration
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
		public string Mode
		{
			get;
			set;
		}

		[DataMember(Order=2, EmitDefaultValue=false)]
		public bool? TreatWarningsAsError
		{
			get;
			set;
		}

		public ChangeConfigurationInput()
		{
		}
	}
}