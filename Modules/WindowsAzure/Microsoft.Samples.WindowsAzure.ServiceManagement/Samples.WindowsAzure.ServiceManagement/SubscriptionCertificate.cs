using System;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class SubscriptionCertificate : IExtensibleDataObject
	{
		[DataMember(Order=3)]
		public string Data
		{
			get;
			set;
		}

		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		[DataMember(Order=1)]
		public string PublicKey
		{
			get;
			set;
		}

		[DataMember(Order=2)]
		public string Thumbprint
		{
			get;
			set;
		}

		public SubscriptionCertificate()
		{
		}
	}
}