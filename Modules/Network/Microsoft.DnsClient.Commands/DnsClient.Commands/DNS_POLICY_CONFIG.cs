using System;

namespace Microsoft.DnsClient.Commands
{
	internal struct DNS_POLICY_CONFIG
	{
		private string keyName;

		private uint dnssecValidationRequired;

		private uint dnssecQueryIPSECRequired;

		private uint dnssecQueryIPSECEncryption;

		private IntPtr dnsQueryIPSECCARestriction;

		private string directAccessDNSServers;

		private uint directAccessQueryIPSECRequired;

		private uint directAccessQueryIPSECEncryption;

		private IntPtr directAccessProxyInformation;

		private uint dnspolicyConfigType;

	}
}