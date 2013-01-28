using System;

namespace Microsoft.DnsClient.Commands
{
	internal struct IP_V4_ADDRESS
	{
		public ushort sin_family;

		public ushort sin_port;

		public byte[] sin_addr;

		public byte[] sin_scope_id;

	}
}