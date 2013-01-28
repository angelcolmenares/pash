using System;

namespace Microsoft.DnsClient.Commands
{
	internal struct DNS_GLOBAL_POLICY_CONFIG
	{
		private uint directAccessState;

		private uint fallbackMode;

		private DNS_DIRECT_ACCESS_QUERY_ORDER directAccessQueryOrder;

	}
}