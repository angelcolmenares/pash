using System;

namespace Microsoft.DnsClient.Commands
{
	internal struct WINS_RecordType
	{
		public uint MappingFlag;

		public uint LookupTimeout;

		public uint CacheTimeout;

		public uint WinsServerCount;

		public IntPtr WinsServers;

	}
}