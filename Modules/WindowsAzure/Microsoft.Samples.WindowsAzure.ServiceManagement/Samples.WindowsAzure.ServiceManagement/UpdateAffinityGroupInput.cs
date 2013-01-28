using System;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[DataContract(Name="UpdateAffinityGroup", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class UpdateAffinityGroupInput : IExtensibleDataObject
	{
		[DataMember(Order=2, EmitDefaultValue=false)]
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

		[DataMember(Order=1)]
		public string Label
		{
			get;
			set;
		}

		[DataMember(Order=3, EmitDefaultValue=false)]
		public string LocationConstraint
		{
			get;
			set;
		}

		public UpdateAffinityGroupInput()
		{
		}
	}
}