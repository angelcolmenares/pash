using System;

namespace Microsoft.DnsClient.Commands
{
	internal struct SOA_RecordType
	{
		public IntPtr NamePrimaryServer;

		public IntPtr NameAdministrator;

		public uint SerialNo;

		public uint Refresh;

		public uint Retry;

		public uint Expire;

		public uint DefaultTtl;

	}
}