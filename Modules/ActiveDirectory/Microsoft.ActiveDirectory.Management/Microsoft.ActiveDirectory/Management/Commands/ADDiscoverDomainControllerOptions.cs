using System;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Flags]
	internal enum ADDiscoverDomainControllerOptions
	{
		None = 0,
		AvoidSelf = 1,
		TryNextClosestSite = 2,
		Writable = 4,
		ForceDiscover = 8,
		ReturnDnsName = 16,
		ReturnFlatName = 32
	}
}