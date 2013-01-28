using System;

namespace Microsoft.DnsClient.Commands
{
	internal struct IP_ADAPTER_UNICAST_ADDRESS
	{
		public ulong Alignment;

		public IntPtr Next;

		public SOCKET_ADDRESS Address;

		public uint PrefixOrigin;

		public uint SuffixOrigin;

		public uint DadState;

		public uint ValidLifetime;

		public uint PreferredLifetime;

		public uint LeaseLifetime;

	}
}