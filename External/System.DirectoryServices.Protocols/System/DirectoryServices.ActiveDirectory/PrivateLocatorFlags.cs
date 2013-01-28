using System;

namespace System.DirectoryServices.ActiveDirectory
{
	[Flags]
	internal enum PrivateLocatorFlags : long
	{
		DirectoryServicesRequired = 16,
		DirectoryServicesPreferred = 32,
		GCRequired = 64,
		PdcRequired = 128,
		BackgroundOnly = 256,
		IPRequired = 512,
		DSWriteableRequired = 4096,
		GoodTimeServerPreferred = 8192,
		OnlyLDAPNeeded = 32768,
		IsFlatName = 65536,
		IsDNSName = 131072,
		ReturnDNSName = 1073741824,
		ReturnFlatName = 2147483648
	}
}