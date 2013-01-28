using System;

namespace Microsoft.DnsClient.Commands
{
	internal struct PREMARSHAL_DNS_RECORD
	{
		public IntPtr pNext;

		public string pName;

		public RecordType wType;

		public ushort wDataLength;

		public DNS_RECORD_FLAGS flags;

		public uint dwTtl;

		public uint dwReserved;

	}
}