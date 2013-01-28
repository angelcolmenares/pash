using System;

namespace Microsoft.ActiveDirectory.Management
{
	internal static class DCLocatorFlagMapping
	{
		public const string ForceRediscovery = "ForceRediscover";

		public const string DirectoryServicesRequired = "MinimumDirectoryServiceVersion:Windows2000";

		public const string DirectoryServicesPreferred = "DirectoryServicesPreferred";

		public const string GCRequired = "GlobalCatalog";

		public const string PDCRequired = "PrimaryDC";

		public const string IPRequired = "IpRequired";

		public const string KDCRequired = "KDC";

		public const string ReliableTimeServer = "ReliableTimeService";

		public const string TimeServerRequired = "TimeService";

		public const string WriteableRequired = "Writable";

		public const string GoodTimeServerPreferred = "ReliableTimeService";

		public const string AvoidSelf = "AvoidSelf";

		public const string OnlyLdapNeeded = "OnlyLdapNeeded";

		public const string IsFlatName = "IsFlatName";

		public const string IsDNSName = "IsDnsName";

		public const string TryNextClosestSite = "NextClosestSite";

		public const string DirectoryServices6Required = "MinimumDirectoryServiceVersion:Windows2008";

		public const string DirectoryServices8Required = "MinimumDirectoryServiceVersion:Windows2012";

		public const string WebServiceRequired = "ADWS";

		public const string ReturnDnsName = "ReturnDnsName";

		public const string ReturnFlatName = "ReturnFlatName";

	}
}