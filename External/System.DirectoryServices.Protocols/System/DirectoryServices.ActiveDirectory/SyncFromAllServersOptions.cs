using System;

namespace System.DirectoryServices.ActiveDirectory
{
	[Flags]
	public enum SyncFromAllServersOptions
	{
		None = 0,
		AbortIfServerUnavailable = 1,
		SyncAdjacentServerOnly = 2,
		CheckServerAlivenessOnly = 8,
		SkipInitialCheck = 16,
		PushChangeOutward = 32,
		CrossSite = 64
	}
}