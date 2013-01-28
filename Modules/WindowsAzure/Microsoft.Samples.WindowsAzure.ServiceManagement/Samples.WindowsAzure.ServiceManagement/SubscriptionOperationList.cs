using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[CollectionDataContract(Namespace="http://schemas.microsoft.com/windowsazure", Name="SubscriptionOperations", ItemName="SubscriptionOperation")]
	public class SubscriptionOperationList : List<SubscriptionOperation>, IExtensibleDataObject
	{
		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		public SubscriptionOperationList()
		{
		}

		public SubscriptionOperationList(IEnumerable<SubscriptionOperation> subscriptions) : base(subscriptions)
		{
		}
	}
}