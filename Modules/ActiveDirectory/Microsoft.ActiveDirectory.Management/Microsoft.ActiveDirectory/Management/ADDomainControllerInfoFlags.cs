using System;

namespace Microsoft.ActiveDirectory.Management
{
	[Flags]
	internal enum ADDomainControllerInfoFlags : uint
	{
		Pdc = 1,
		Gc = 4,
		LdapServer = 8,
		DirectoryServer = 16,
		Kdc = 32,
		TimeService = 64,
		ClosestSiteToClient = 128,
		Writable = 256,
		ReliableTimeService = 512,
		ApplicationNC = 1024,
		PartialSecrets_6 = 2048,
		FullSecrets_6 = 4096,
		ADWebService = 8192,
		Pings = 1048575,
		DCNameIsDNS = 536870912,
		DomainNameIsDNS = 1073741824,
		ForestNameIsDNS = 2147483648
	}
}