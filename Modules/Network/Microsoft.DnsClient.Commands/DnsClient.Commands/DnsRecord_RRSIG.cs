using System;

namespace Microsoft.DnsClient.Commands
{
	public class DnsRecord_RRSIG : DnsRecord
	{
		public RecordType TypeCovered;

		public EncryptionAlgorithm Algorithm;

		public byte LabelCount;

		public uint OriginalTtl;

		public DateTime Expiration;

		public DateTime Signed;

		public ushort KeyTag;

		public string Signer;

		public byte[] Signature;

		public DnsRecord_RRSIG()
		{
		}
	}
}