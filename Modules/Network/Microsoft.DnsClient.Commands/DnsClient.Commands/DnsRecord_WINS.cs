using System;

namespace Microsoft.DnsClient.Commands
{
	public class DnsRecord_WINS : DnsRecord
	{
		public WINSMappingFlag MappingFlag;

		public uint LookupTimeout;

		public uint CacheTimeout;

		public string[] IP4Addresses;

		public DnsRecord_WINS()
		{
		}
	}
}