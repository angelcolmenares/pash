using System;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class StorageDomain : IExtensibleDataObject
	{
		[DataMember(Order=2)]
		public string DomainName
		{
			get;
			set;
		}

		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		[DataMember(Order=1, EmitDefaultValue=false)]
		public string ServiceName
		{
			get;
			set;
		}

		public StorageDomain()
		{
		}
	}
}