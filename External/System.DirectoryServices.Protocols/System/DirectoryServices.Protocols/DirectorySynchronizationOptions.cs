using System;

namespace System.DirectoryServices.Protocols
{
	[Flags]
	public enum DirectorySynchronizationOptions : long
	{
		None = 0,
		ObjectSecurity = 1,
		ParentsFirst = 2048,
		PublicDataOnly = 8192,
		IncrementalValues = 2147483648
	}
}