using System;

namespace Microsoft.DnsClient.Commands
{
	public class DnsRecord_SOA : DnsRecord
	{
		public string PrimaryServer;

		public string NameAdministrator;

		public uint SerialNumber;

		public uint TimeToZoneRefresh;

		public uint TimeToZoneFailureRetry;

		public uint TimeToExpiration;

		public uint DefaultTTL;

		public DnsRecord_SOA()
		{
		}
	}
}