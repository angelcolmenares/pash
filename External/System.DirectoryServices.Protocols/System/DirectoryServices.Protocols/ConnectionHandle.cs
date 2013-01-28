using Microsoft.Win32.SafeHandles;
using System;
using System.Security;

namespace System.DirectoryServices.Protocols
{
	[SuppressUnmanagedCodeSecurity]
	internal sealed class ConnectionHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		internal ConnectionHandle() : base(true)
		{
			base.SetHandle(Wldap32.ldap_init(null, 0x185));
			if (this.handle != (IntPtr)0)
			{
				return;
			}
			else
			{
				int num = Wldap32.LdapGetLastError();
				if (!Utility.IsLdapError((LdapError)num))
				{
					throw new LdapException(num);
				}
				else
				{
					string str = LdapErrorMappings.MapResultCode(num);
					throw new LdapException(num, str);
				}
			}
		}

		internal ConnectionHandle(IntPtr value) : base(true)
		{
			if (value != (IntPtr)0)
			{
				base.SetHandle(value);
				return;
			}
			else
			{
				int num = Wldap32.LdapGetLastError();
				if (!Utility.IsLdapError((LdapError)num))
				{
					throw new LdapException(num);
				}
				else
				{
					string str = LdapErrorMappings.MapResultCode(num);
					throw new LdapException(num, str);
				}
			}
		}

		protected override bool ReleaseHandle()
		{
			if (this.handle != (IntPtr)0)
			{
				Wldap32.ldap_unbind(this.handle);
				this.handle = (IntPtr)0;
			}
			return true;
		}
	}
}