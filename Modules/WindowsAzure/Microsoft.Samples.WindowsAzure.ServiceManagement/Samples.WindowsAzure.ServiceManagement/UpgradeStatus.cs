using System;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class UpgradeStatus : IExtensibleDataObject
	{
		[DataMember(Order=3)]
		public int CurrentUpgradeDomain
		{
			get;
			set;
		}

		[DataMember(Order=2)]
		public string CurrentUpgradeDomainState
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
		public string UpgradeType
		{
			get;
			set;
		}

		public UpgradeStatus()
		{
		}
	}
}