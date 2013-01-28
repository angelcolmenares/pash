using System;

namespace Microsoft.DnsClient.Commands
{
	public class DnsRecord_SIG : DnsRecord
	{
		public string NameSigner;

		public RecordType TypeCovered;

		public EncryptionAlgorithm Algorithm;

		public byte LabelCount;

		public uint OriginalTTL;

		public DateTime SignatureExpirationDate;

		public DateTime SignatureValidDate;

		public ushort KeyTag;

		public string Signature;

		public DnsRecord_SIG()
		{
		}
	}
}