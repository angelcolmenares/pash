using System;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[DataContract(Name="ParentMachineImage", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class SetParentImageInput : IExtensibleDataObject
	{
		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		[DataMember(Order=1)]
		public string ParentImageName
		{
			get;
			set;
		}

		public SetParentImageInput()
		{
		}
	}
}