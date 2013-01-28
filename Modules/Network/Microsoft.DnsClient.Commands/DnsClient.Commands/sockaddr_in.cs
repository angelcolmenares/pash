using System;

namespace Microsoft.DnsClient.Commands
{
	public struct sockaddr_in
	{
		private ushort Family;

		private ushort sin6_port;

		private uint IP4Addr;

		private char[] zero;

	}
}