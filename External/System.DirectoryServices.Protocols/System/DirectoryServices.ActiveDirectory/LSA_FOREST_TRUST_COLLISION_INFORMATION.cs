using System;

namespace System.DirectoryServices.ActiveDirectory
{
	internal sealed class LSA_FOREST_TRUST_COLLISION_INFORMATION
	{
		public int RecordCount;

		public IntPtr Entries;

		public LSA_FOREST_TRUST_COLLISION_INFORMATION()
		{
		}
	}
}