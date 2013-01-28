using System;

namespace Microsoft.ActiveDirectory.Management
{
	[Flags]
	internal enum MSA_INFO_STATE : uint
	{
		MsaInfoNotExist = 1,
		MsaInfoNotService = 2,
		MsaInfoCannotInstall = 3,
		MsaInfoCanInstall = 4,
		MsaInfoInstalled = 5
	}
}