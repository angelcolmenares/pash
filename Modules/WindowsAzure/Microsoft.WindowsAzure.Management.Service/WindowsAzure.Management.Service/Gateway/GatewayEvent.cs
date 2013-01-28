using System;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.Management.Service.Gateway
{
	[DataContract(Name="GatewayEvent", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class GatewayEvent
	{
		[DataMember(IsRequired=false, EmitDefaultValue=false)]
		public string Data
		{
			get;
			set;
		}

		[DataMember]
		public int Id
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

		[DataMember]
		public DateTime Timestamp
		{
			get;
			set;
		}

		public GatewayEvent()
		{
		}
	}
}