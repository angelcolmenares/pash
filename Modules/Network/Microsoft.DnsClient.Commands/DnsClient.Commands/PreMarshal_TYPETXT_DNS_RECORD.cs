using System;

namespace Microsoft.DnsClient.Commands
{
	internal struct PreMarshal_TYPETXT_DNS_RECORD
	{
		public IntPtr pNext;

		public IntPtr pName;

		public ushort wType;

		public ushort wDataLength;

		public DNS_RECORD_FLAGS flags;

		public uint dwTtl;

		public uint dwReserved;

		public PreMarshal_TXT_RecordType TXTtype;

	}
}