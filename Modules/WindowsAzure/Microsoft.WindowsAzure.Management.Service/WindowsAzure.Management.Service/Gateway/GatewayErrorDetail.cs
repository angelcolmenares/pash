using System;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.Management.Service.Gateway
{
	[DataContract(Name="Error", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class GatewayErrorDetail
	{
		[DataMember]
		public string Code
		{
			get;
			set;
		}

		[DataMember]
		public string Message
		{
			get;
			set;
		}

		public GatewayErrorDetail(GatewayErrorCode code, string message)
		{
			this.Code = code.ToString();
			this.Message = message;
		}
	}
}