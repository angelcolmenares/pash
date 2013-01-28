using System;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.Management.Service.Gateway
{
	[DataContract(Name="UpdateConnection", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class UpdateConnection
	{
		[DataMember]
		public string IPAddress
		{
			get;
			set;
		}

		[DataMember]
		public UpdateConnectionOperation Operation
		{
			get;
			set;
		}

		public UpdateConnection()
		{
		}
	}
}