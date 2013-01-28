using System;

namespace Microsoft.DnsClient.Commands
{
	public class DnsRecord_DS : DnsRecord
	{
		public ushort KeyTag;

		public EncryptionAlgorithm Algorithm;

		public DigestType DType;

		public byte[] Digest;

		public DnsRecord_DS()
		{
		}
	}
}