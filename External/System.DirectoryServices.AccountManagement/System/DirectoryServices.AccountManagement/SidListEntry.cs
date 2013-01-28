using System;
using System.Runtime.InteropServices;
using System.Security;

namespace System.DirectoryServices.AccountManagement
{
	internal class SidListEntry : IDisposable
	{
		public IntPtr pSid;

		public string name;

		public string sidIssuerName;

		public SidListEntry()
		{
			this.pSid = IntPtr.Zero;
		}

		[SecurityCritical]
		public virtual void Dispose()
		{
			if (this.pSid != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(this.pSid);
				this.pSid = IntPtr.Zero;
			}
		}
	}
}