using System;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class Gateway : IExtensibleDataObject
	{
		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=1)]
		public string Profile
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=2)]
		public LocalNetworkSiteList Sites
		{
			get;
			set;
		}

		public Gateway()
		{
		}
	}
}