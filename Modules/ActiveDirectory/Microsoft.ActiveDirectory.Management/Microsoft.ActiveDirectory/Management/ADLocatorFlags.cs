using System;

namespace Microsoft.ActiveDirectory.Management
{
	[Flags]
	internal enum ADLocatorFlags : uint
	{
		None = 0,
		ForceRediscovery = 1,
		DirectoryServicesRequired = 16,
		DirectoryServicesPreferred = 32,
		GCRequired = 64,
		PdcRequired = 128,
		IPRequired = 512,
		KdcRequired = 1024,
		TimeServerRequired = 2048,
		WriteableRequired = 4096,
		GoodTimeServerPreferred = 8192,
		AvoidSelf = 16384,
		OnlyLdapNeeded = 32768,
		IsFlatName = 65536,
		IsDnsName = 131072,
		TryNextClosestSite = 262144,
		DirectoryServices6Required = 524288,
		WebServiceRequired = 1048576,
		DirectoryServices8Required = 2097152,
		ReturnDnsName = 1073741824,
		ReturnFlatName = 2147483648
	}
}