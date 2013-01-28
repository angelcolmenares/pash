using System;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class SubscriptionOperationCaller : IExtensibleDataObject
	{
		[DataMember(Order=3, EmitDefaultValue=false)]
		public string ClientIP
		{
			get;
			set;
		}

		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		[DataMember(Order=2, EmitDefaultValue=false)]
		public string SubscriptionCertificateThumbprint
		{
			get;
			set;
		}

		[DataMember(Order=0)]
		public bool UsedServiceManagementApi
		{
			get;
			set;
		}

		[DataMember(Order=1, EmitDefaultValue=false)]
		public string UserEmailAddress
		{
			get;
			set;
		}

		public SubscriptionOperationCaller()
		{
		}
	}
}