using System;

namespace System.DirectoryServices.ActiveDirectory
{
	internal sealed class SID_AND_ATTR
	{
		public IntPtr pSid;

		public int attrs;

		public SID_AND_ATTR()
		{
			this.pSid = IntPtr.Zero;
		}
	}
}