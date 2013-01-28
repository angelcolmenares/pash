using System;

namespace Microsoft.DnsClient.Commands
{
	public class DnsRecord_WKS : DnsRecord
	{
		public string IP4Address;

		public IpProtocol Protocol;

		public byte[] Bitmask;

		public DnsRecord_WKS()
		{
		}
	}
}