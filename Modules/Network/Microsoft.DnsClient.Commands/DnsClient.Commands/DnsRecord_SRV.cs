using System;

namespace Microsoft.DnsClient.Commands
{
	public class DnsRecord_SRV : DnsRecord
	{
		public string NameTarget;

		public ushort Priority;

		public ushort Weight;

		public ushort Port;

		public DnsRecord_SRV()
		{
		}
	}
}