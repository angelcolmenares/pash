using System;

namespace System.DirectoryServices.ActiveDirectory
{
	internal sealed class DS_REPL_ATTR_META_DATA_2
	{
		public IntPtr pszAttributeName;

		public int dwVersion;

		public int ftimeLastOriginatingChange1;

		public int ftimeLastOriginatingChange2;

		public Guid uuidLastOriginatingDsaInvocationID;

		public long usnOriginatingChange;

		public long usnLocalChange;

		public IntPtr pszLastOriginatingDsaDN;

		public DS_REPL_ATTR_META_DATA_2()
		{
		}
	}
}