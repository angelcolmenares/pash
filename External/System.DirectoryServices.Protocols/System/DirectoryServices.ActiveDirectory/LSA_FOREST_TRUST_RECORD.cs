using System;
using System.Runtime.InteropServices;

namespace System.DirectoryServices.ActiveDirectory
{
	[StructLayout(LayoutKind.Explicit)]
	internal sealed class LSA_FOREST_TRUST_RECORD
	{
		[FieldOffset(0)]
		public int Flags;

		[FieldOffset(4)]
		public LSA_FOREST_TRUST_RECORD_TYPE ForestTrustType;

		[FieldOffset(8)]
		public LARGE_INTEGER Time;

		[FieldOffset(16)]
		public LSA_UNICODE_STRING TopLevelName;

		[FieldOffset(16)]
		public LSA_FOREST_TRUST_BINARY_DATA Data;

		[FieldOffset(16)]
		public LSA_FOREST_TRUST_DOMAIN_INFO DomainInfo;

		public LSA_FOREST_TRUST_RECORD()
		{
		}
	}
}