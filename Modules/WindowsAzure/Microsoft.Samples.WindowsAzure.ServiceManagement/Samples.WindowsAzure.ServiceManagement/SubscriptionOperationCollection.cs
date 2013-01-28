using System;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class SubscriptionOperationCollection : IExtensibleDataObject
	{
		[DataMember(Order=1, EmitDefaultValue=false)]
		public string ContinuationToken
		{
			get;
			set;
		}

		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		[DataMember(Order=0)]
		public SubscriptionOperationList SubscriptionOperations
		{
			get;
			set;
		}

		public SubscriptionOperationCollection()
		{
		}
	}
}