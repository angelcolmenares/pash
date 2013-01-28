using System;
using System.Runtime.InteropServices;

namespace System.DirectoryServices.Protocols
{
	internal sealed class SafeBerval
	{
		public int bv_len;

		public IntPtr bv_val;

		public SafeBerval()
		{
			this.bv_val = (IntPtr)0;
		}

		~SafeBerval()
		{
			try
			{
				if (this.bv_val != (IntPtr)0)
				{
					Marshal.FreeHGlobal(this.bv_val);
				}
			}
			finally
			{
				//this.Finalize();
			}
		}
	}
}