using System;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[DataContract(Name="CreateAffinityGroup", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class CreateAffinityGroupInput : IExtensibleDataObject
	{
		[DataMember(Order=3, EmitDefaultValue=false)]
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

		[DataMember(Order=1)]
		public string Name
		{
			get;
			set;
		}

		public CreateAffinityGroupInput()
		{
		}
	}
}