using System;

namespace Microsoft.DnsClient.Commands
{
	public struct sockaddr_in6
	{
		internal ushort sin6_family;

		private ushort sin6_port;

		public byte[] IP4Addr;

		public byte[] IP6Address;

		private uint sin6_scope_id;

	}
}