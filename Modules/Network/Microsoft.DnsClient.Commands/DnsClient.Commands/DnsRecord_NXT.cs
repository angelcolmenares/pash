using System;

namespace Microsoft.DnsClient.Commands
{
	public class DnsRecord_NXT : DnsRecord
	{
		public string NameNext;

		public RecordType[] Types;

		public DnsRecord_NXT()
		{
		}
	}
}