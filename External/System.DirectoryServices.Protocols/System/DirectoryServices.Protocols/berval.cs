using System;

namespace System.DirectoryServices.Protocols
{
	internal sealed class berval
	{
		public int bv_len;

		public IntPtr bv_val;

		public berval()
		{
			this.bv_val = (IntPtr)0;
		}
	}
}