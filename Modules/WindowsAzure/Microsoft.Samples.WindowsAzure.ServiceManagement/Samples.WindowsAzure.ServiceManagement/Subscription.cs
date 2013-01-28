using System;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class Subscription : IExtensibleDataObject
	{
		[DataMember(Order=3)]
		public string AccountAdminLiveEmailId
		{
			get;
			set;
		}

		[DataMember(Order=8)]
		public int CurrentCoreCount
		{
			get;
			set;
		}

		[DataMember(Order=16, EmitDefaultValue=false)]
		public int CurrentDnsServers
		{
			get;
			set;
		}

		[DataMember(Order=9)]
		public int CurrentHostedServices
		{
			get;
			set;
		}

		[DataMember(Order=14, EmitDefaultValue=false)]
		public int CurrentLocalNetworkSites
		{
			get;
			set;
		}

		[DataMember(Order=10)]
		public int CurrentStorageAccounts
		{
			get;
			set;
		}

		[DataMember(Order=12, EmitDefaultValue=false)]
		public int CurrentVirtualNetworkSites
		{
			get;
			set;
		}

		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		[DataMember(Order=5)]
		public int MaxCoreCount
		{
			get;
			set;
		}

		[DataMember(Order=15, EmitDefaultValue=false)]
		public int MaxDnsServers
		{
			get;
			set;
		}

		[DataMember(Order=7)]
		public int MaxHostedServices
		{
			get;
			set;
		}

		[DataMember(Order=13, EmitDefaultValue=false)]
		public int MaxLocalNetworkSites
		{
			get;
			set;
		}

		[DataMember(Order=6)]
		public int MaxStorageAccounts
		{
			get;
			set;
		}

		[DataMember(Order=11, EmitDefaultValue=false)]
		public int MaxVirtualNetworkSites
		{
			get;
			set;
		}

		[DataMember(Order=4)]
		public string ServiceAdminLiveEmailId
		{
			get;
			set;
		}

		[DataMember(Order=0)]
		public string SubscriptionID
		{
			get;
			set;
		}

		[DataMember(Order=1)]
		public string SubscriptionName
		{
			get;
			set;
		}

		[DataMember(Order=2)]
		public string SubscriptionStatus
		{
			get;
			set;
		}

		public Subscription()
		{
		}
	}
}