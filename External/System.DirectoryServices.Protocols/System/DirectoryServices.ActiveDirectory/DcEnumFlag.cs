using System;

namespace System.DirectoryServices.ActiveDirectory
{
	[Flags]
	internal enum DcEnumFlag
	{
		OnlyDoSiteName = 1,
		NotifyAfterSiteRecords = 2
	}
}