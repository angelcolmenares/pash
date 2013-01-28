using System;

namespace Microsoft.DnsClient.Commands
{
	public class DnsRecord_MX : DnsRecord
	{
		public string NameExchange;

		public ushort Preference;

		public DnsRecord_MX()
		{
		}
	}
}