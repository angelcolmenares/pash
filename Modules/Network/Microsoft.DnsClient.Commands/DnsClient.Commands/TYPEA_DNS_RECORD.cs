using System;

namespace Microsoft.DnsClient.Commands
{
	internal struct TYPEA_DNS_RECORD
	{
		public IntPtr pNext;

		public IntPtr pName;

		public ushort wType;

		public ushort wDataLength;

		public DNS_RECORD_FLAGS flags;

		public uint dwTtl;

		public uint dwReserved;

		public A_RecordType typeA;

	}
}