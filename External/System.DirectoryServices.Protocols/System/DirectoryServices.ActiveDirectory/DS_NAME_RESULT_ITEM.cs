using System;

namespace System.DirectoryServices.ActiveDirectory
{
	internal sealed class DS_NAME_RESULT_ITEM
	{
		public DS_NAME_ERROR status;

		public IntPtr pDomain;

		public IntPtr pName;

		public DS_NAME_RESULT_ITEM()
		{
		}
	}
}