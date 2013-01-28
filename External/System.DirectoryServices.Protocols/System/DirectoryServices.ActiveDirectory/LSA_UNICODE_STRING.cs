using System;

namespace System.DirectoryServices.ActiveDirectory
{
	internal sealed class LSA_UNICODE_STRING
	{
		public short Length;

		public short MaximumLength;

		public IntPtr Buffer;

		public LSA_UNICODE_STRING()
		{
		}
	}
}