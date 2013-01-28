using System;

namespace System.DirectoryServices.ActiveDirectory
{
	internal sealed class LSA_FOREST_TRUST_BINARY_DATA
	{
		public int Length;

		public IntPtr Buffer;

		public LSA_FOREST_TRUST_BINARY_DATA()
		{
		}
	}
}