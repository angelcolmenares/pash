using System;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.Management.Service
{
	[DataContract(Name="GatewayOperationAsyncResponse", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class GatewayOperationAsyncResponse : IExtensibleDataObject
	{
		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=1)]
		public string ID
		{
			get;
			set;
		}

		public GatewayOperationAsyncResponse()
		{
		}
	}
}