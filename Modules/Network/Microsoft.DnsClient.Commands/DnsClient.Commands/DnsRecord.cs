using System;

namespace Microsoft.DnsClient.Commands
{
	public class DnsRecord
	{
		public string Name;

		public RecordType Type;

		public DNSCharset CharacterSet;

		public DNSSection Section;

		public ushort DataLength;

		public uint TTL;

		public DnsRecord()
		{
		}
	}
}