using System;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[DataContract(Name="SetMachineImageProperties", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class SetMachineImagePropertiesInput : IExtensibleDataObject
	{
		[DataMember(Order=2)]
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

		public SetMachineImagePropertiesInput()
		{
		}
	}
}