using System;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.Management.Service.Gateway
{
	[DataContract(Name="SharedKey", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class SharedKey
	{
		[DataMember]
		public string Value
		{
			get;
			set;
		}

		public SharedKey()
		{
		}
	}
}