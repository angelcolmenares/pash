using System;

namespace Microsoft.DnsClient.Commands
{
	internal struct IP_V6_ADDRESS
	{
		public ushort sin6_family;

		public ushort sin6_port;

		public uint sin6_flowinfo;

		public byte[] sin6_addr;

		public uint sin6_scope_id;

	}
}