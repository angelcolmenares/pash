using System;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[DataContract(Name="Swap", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class SwapDeploymentInput : IExtensibleDataObject
	{
		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		[DataMember(Order=1)]
		public string Production
		{
			get;
			set;
		}

		[DataMember(Order=2)]
		public string SourceDeployment
		{
			get;
			set;
		}

		public SwapDeploymentInput()
		{
		}
	}
}