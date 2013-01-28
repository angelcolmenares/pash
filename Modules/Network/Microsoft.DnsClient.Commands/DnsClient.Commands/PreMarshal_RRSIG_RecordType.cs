using System;

namespace Microsoft.DnsClient.Commands
{
	internal struct PreMarshal_RRSIG_RecordType
	{
		public ushort TypeCovered;

		public byte Algorithm;

		public byte LabelCount;

		public uint OriginalTtl;

		public uint Expiration;

		public uint TimeSigned;

		public ushort KeyTag;

		public ushort SignatureLength;

		public IntPtr NameSigner;

	}
}