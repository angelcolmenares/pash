using System;

namespace Microsoft.DnsClient.Commands
{
	internal struct DS_RecordType
	{
		public ushort KeyTag;

		public byte Algorithm;

		public byte DigestType;

		public ushort DigestLength;

		public ushort Pad;

		public byte Digest;

	}
}