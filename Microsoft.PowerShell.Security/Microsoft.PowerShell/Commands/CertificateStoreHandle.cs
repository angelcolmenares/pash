using System;
using System.Management.Automation.Security;
using System.Runtime.InteropServices;

namespace Microsoft.PowerShell.Commands
{
	internal sealed class CertificateStoreHandle : SafeHandle
	{
		public IntPtr Handle
		{
			get
			{
				return this.handle;
			}
			set
			{
				this.handle = value;
			}
		}

		public override bool IsInvalid
		{
			get
			{
				return this.handle == IntPtr.Zero;
			}
		}

		public CertificateStoreHandle() : base(IntPtr.Zero, true)
		{
		}

		protected override bool ReleaseHandle()
		{
			bool flag = false;
			if (IntPtr.Zero != this.handle)
			{
				flag = NativeMethods.CertCloseStore(this.handle, 0);
				this.handle = IntPtr.Zero;
			}
			return flag;
		}
	}
}