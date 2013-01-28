using System;

namespace Microsoft.DnsClient.Commands
{
	public class DnsRecord_NSEC : DnsRecord
	{
		public string NextDomainName;

		public byte[] TypeBitMap;

		public DnsRecord_NSEC()
		{
		}
	}
}