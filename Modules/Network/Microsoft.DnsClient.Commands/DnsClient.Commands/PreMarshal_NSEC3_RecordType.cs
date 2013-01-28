using System;

namespace Microsoft.DnsClient.Commands
{
	internal struct PreMarshal_NSEC3_RecordType
	{
		public byte Algorithm;

		public byte Flags;

		public ushort Iterations;

		public byte SaltLength;

		public byte HashLength;

		public ushort TypeBitMapsLength;

	}
}