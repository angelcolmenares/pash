using System;

namespace System.DirectoryServices.ActiveDirectory
{
	internal sealed class DS_REPSYNCALL_UPDATE
	{
		public SyncFromAllServersEvent eventType;

		public IntPtr pErrInfo;

		public IntPtr pSync;

		public DS_REPSYNCALL_UPDATE()
		{
		}
	}
}