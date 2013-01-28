using System;
using System.Management.Automation.Security;
using System.Runtime.InteropServices;

namespace Microsoft.PowerShell.Commands
{
	internal sealed class CertificateFilterHandle : SafeHandle
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

		public CertificateFilterHandle() : base(IntPtr.Zero, true)
		{
		}

		protected override bool ReleaseHandle()
		{
			bool flag = false;
			if (IntPtr.Zero != this.handle)
			{
				NativeMethods.CCFindCertificateFreeFilter(this.handle);
				this.handle = IntPtr.Zero;
				flag = true;
			}
			return flag;
		}
	}
}