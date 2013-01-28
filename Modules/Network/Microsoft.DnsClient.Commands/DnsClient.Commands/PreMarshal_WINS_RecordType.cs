using System;

namespace Microsoft.DnsClient.Commands
{
	internal struct PreMarshal_WINS_RecordType
	{
		public uint MappingFlag;

		public uint LookupTimeout;

		public uint CacheTimeout;

		public uint WinsServerCount;

	}
}