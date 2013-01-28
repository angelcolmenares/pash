using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class DnsSettings : IExtensibleDataObject
	{
		[DataMember(EmitDefaultValue=false, Order=1)]
		public DnsServerList DnsServers
		{
			get;
			set;
		}

		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		public DnsSettings()
		{
		}
	}
}