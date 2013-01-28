using System;

namespace Microsoft.DnsClient.Commands
{
	public class DnsRecord_DNSKEY : DnsRecord
	{
		public ushort Flags;

		public KeyProtocol Protocol;

		public EncryptionAlgorithm Algorithm;

		public byte[] Key;

		public DnsRecord_DNSKEY()
		{
		}
	}
}