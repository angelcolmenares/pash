using System;
using System.Net;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.Management.Service.Gateway
{
	[DataContract(Name="GatewayOperation", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class GatewayOperation
	{
		[DataMember(IsRequired=false, EmitDefaultValue=false)]
		public string Data
		{
			get;
			set;
		}

		[DataMember(IsRequired=false, EmitDefaultValue=false)]
		public GatewayErrorDetail Error
		{
			get;
			set;
		}

		[DataMember(IsRequired=false, EmitDefaultValue=false)]
		public HttpStatusCode HttpStatusCode
		{
			get;
			set;
		}

		[DataMember]
		public string ID
		{
			get;
			set;
		}

		[DataMember]
		public GatewayOperationStatus Status
		{
			get;
			set;
		}

		public GatewayOperation()
		{
		}
	}
}