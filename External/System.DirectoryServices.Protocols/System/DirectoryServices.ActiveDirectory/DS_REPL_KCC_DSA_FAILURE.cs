using System;

namespace System.DirectoryServices.ActiveDirectory
{
	internal sealed class DS_REPL_KCC_DSA_FAILURE
	{
		public IntPtr pszDsaDN;

		public Guid uuidDsaObjGuid;

		public long ftimeFirstFailure;

		public int cNumFailures;

		public int dwLastResult;

		public DS_REPL_KCC_DSA_FAILURE()
		{
		}
	}
}