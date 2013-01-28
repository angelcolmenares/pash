using System;

namespace Microsoft.DnsClient.Commands
{
	internal struct PreMarshal_NSEC3PARAM_RecordType
	{
		public byte Algorithm;

		public byte Flags;

		public ushort Iterations;

		public byte SaltLength;

		public byte Pad1;

		public byte Pad2;

		public byte Pad3;

	}
}