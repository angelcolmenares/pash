using System;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class StorageServiceKeys : IExtensibleDataObject
	{
		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		[DataMember(Order=1)]
		public string Primary
		{
			get;
			set;
		}

		[DataMember(Order=2)]
		public string Secondary
		{
			get;
			set;
		}

		public StorageServiceKeys()
		{
		}
	}
}