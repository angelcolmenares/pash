using System;

namespace Microsoft.DnsClient.Commands
{
	internal struct DnsAddrArray
	{
		internal uint MaxCount;

		internal int AddrCount;

		private uint Tag;

		internal ushort Family;

		private ushort Reserved;

		private uint Flags;

		private uint MatchFlag;

		private uint Reserved1;

		private uint Reserved2;

		internal DnsAddr[] DnsAddr;

	}
}