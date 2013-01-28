using System;

namespace Microsoft.DnsClient.Commands
{
	internal struct PreMarshal_DNSKEY_RecordType
	{
		public ushort Flags;

		public byte Protocol;

		public byte Algorithm;

		public ushort KeyLength;

		public ushort Pad;

	}
}