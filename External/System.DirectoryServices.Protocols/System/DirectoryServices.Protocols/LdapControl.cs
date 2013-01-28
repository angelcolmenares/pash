using System;

namespace System.DirectoryServices.Protocols
{
	internal sealed class LdapControl
	{
		public IntPtr ldctl_oid;

		public berval ldctl_value;

		public bool ldctl_iscritical;

		public LdapControl()
		{
			this.ldctl_oid = (IntPtr)0;
		}
	}
}