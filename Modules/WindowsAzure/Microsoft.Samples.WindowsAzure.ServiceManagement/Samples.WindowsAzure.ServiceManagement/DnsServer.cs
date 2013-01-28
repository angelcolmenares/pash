using System;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class DnsServer : IExtensibleDataObject
	{
		[DataMember(EmitDefaultValue=false, Order=2)]
		public string Address
		{
			get;
			set;
		}

		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=1)]
		public string Name
		{
			get;
			set;
		}

		public DnsServer()
		{
		}
	}
}