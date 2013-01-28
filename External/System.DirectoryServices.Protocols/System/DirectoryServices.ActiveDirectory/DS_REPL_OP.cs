using System;

namespace System.DirectoryServices.ActiveDirectory
{
	internal sealed class DS_REPL_OP
	{
		public long ftimeEnqueued;

		public int ulSerialNumber;

		public int ulPriority;

		public ReplicationOperationType OpType;

		public int ulOptions;

		public IntPtr pszNamingContext;

		public IntPtr pszDsaDN;

		public IntPtr pszDsaAddress;

		public Guid uuidNamingContextObjGuid;

		public Guid uuidDsaObjGuid;

		public DS_REPL_OP()
		{
		}
	}
}