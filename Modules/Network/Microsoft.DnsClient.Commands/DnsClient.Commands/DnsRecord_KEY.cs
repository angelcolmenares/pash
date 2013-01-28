using System;

namespace Microsoft.DnsClient.Commands
{
	public class DnsRecord_KEY : DnsRecord
	{
		public ushort Flags;

		public KeyProtocol Protocol;

		public EncryptionAlgorithm Algorithm;

		public string PublicKey;

		public DnsRecord_KEY()
		{
		}
	}
}