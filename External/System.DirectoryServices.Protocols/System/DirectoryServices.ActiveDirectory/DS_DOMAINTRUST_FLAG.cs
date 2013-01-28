using System;

namespace System.DirectoryServices.ActiveDirectory
{
	[Flags]
	internal enum DS_DOMAINTRUST_FLAG
	{
		DS_DOMAIN_IN_FOREST = 1,
		DS_DOMAIN_DIRECT_OUTBOUND = 2,
		DS_DOMAIN_TREE_ROOT = 4,
		DS_DOMAIN_PRIMARY = 8,
		DS_DOMAIN_NATIVE_MODE = 16,
		DS_DOMAIN_DIRECT_INBOUND = 32
	}
}