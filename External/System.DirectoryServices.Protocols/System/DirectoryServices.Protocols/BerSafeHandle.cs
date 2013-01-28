using Microsoft.Win32.SafeHandles;
using System;
using System.Security;

namespace System.DirectoryServices.Protocols
{
	[SuppressUnmanagedCodeSecurity]
	internal sealed class BerSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		internal BerSafeHandle() : base(true)
		{
			base.SetHandle(Wldap32.ber_alloc(1));
			if (this.handle != (IntPtr)0)
			{
				return;
			}
			else
			{
				throw new OutOfMemoryException();
			}
		}

		internal BerSafeHandle(berval value) : base(true)
		{
			base.SetHandle(Wldap32.ber_init(value));
			if (this.handle != (IntPtr)0)
			{
				return;
			}
			else
			{
				throw new BerConversionException();
			}
		}

		protected override bool ReleaseHandle()
		{
			Wldap32.ber_free(this.handle, 1);
			return true;
		}
	}
}