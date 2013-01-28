using System;
using System.Runtime.InteropServices;

namespace System.DirectoryServices.Protocols
{
	internal sealed class LdapMod
	{
		public int type;

		public IntPtr attribute;

		public IntPtr values;

		public LdapMod()
		{
			this.attribute = (IntPtr)0;
			this.values = (IntPtr)0;
		}

		~LdapMod()
		{
			try
			{
				if (this.attribute != (IntPtr)0)
				{
					Marshal.FreeHGlobal(this.attribute);
				}
				if (this.values != (IntPtr)0)
				{
					Marshal.FreeHGlobal(this.values);
				}
			}
			finally
			{
				//this.Finalize();
			}
		}
	}
}