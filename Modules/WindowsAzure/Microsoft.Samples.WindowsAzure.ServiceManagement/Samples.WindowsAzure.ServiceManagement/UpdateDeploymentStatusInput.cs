using System;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[DataContract(Name="UpdateDeploymentStatus", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class UpdateDeploymentStatusInput : IExtensibleDataObject
	{
		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		[DataMember(Order=1)]
		public string Status
		{
			get;
			set;
		}

		public UpdateDeploymentStatusInput()
		{
		}
	}
}