using System;

namespace System.DirectoryServices.ActiveDirectory
{
	[Flags]
	public enum LocatorOptions : long
	{
		ForceRediscovery = 1,
		KdcRequired = 1024,
		TimeServerRequired = 2048,
		WriteableRequired = 4096,
		AvoidSelf = 16384
	}
}