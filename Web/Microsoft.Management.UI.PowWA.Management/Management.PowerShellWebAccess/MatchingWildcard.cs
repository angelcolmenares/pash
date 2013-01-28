using System;

namespace Microsoft.Management.PowerShellWebAccess
{
	[Flags]
	internal enum MatchingWildcard
	{
		None = 0,
		User = 1,
		Destination = 2,
		Configuration = 4
	}
}