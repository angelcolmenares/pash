using System;

namespace System.DirectoryServices.ActiveDirectory
{
	internal sealed class DS_REPSYNCALL_ERRINFO
	{
		public IntPtr pszSvrId;

		public SyncFromAllServersErrorCategory error;

		public int dwWin32Err;

		public IntPtr pszSrcId;

		public DS_REPSYNCALL_ERRINFO()
		{
		}
	}
}