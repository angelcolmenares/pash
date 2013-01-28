using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Microsoft.DnsClient.Commands
{
	internal struct DnsExtraInfo
	{
		public int Version;

		public uint Size;

		public IntPtr pNext;

		public uint ID;

		public uint Reserved;

		public IntPtr ServerList;

		public DnsExtraInfo(IPAddress[] DnsServers)
		{
			this.ID = 10;
			this.Size = 0;
			this.Version = -2147483647;
			this.Reserved = 0;
			this.pNext = IntPtr.Zero;
			DnsAddrArray length = new DnsAddrArray();
			length.AddrCount = (int)DnsServers.Length;
			length.MaxCount = 5;
			length.DnsAddr = new DnsAddr[5];
			for (int i = 0; i < (int)DnsServers.Length && i < 5; i++)
			{
				length.Family = (ushort)DnsServers[i].AddressFamily;
				length.DnsAddr[i].SockAddr.sin6_family = (ushort)DnsServers[i].AddressFamily;
				if (DnsServers[i].AddressFamily != AddressFamily.InterNetworkV6)
				{
					length.DnsAddr[i].SockAddr.IP4Addr = DnsServers[i].GetAddressBytes();
					length.DnsAddr[i].SockAddrLength = Marshal.SizeOf(typeof(sockaddr_in));
				}
				else
				{
					length.DnsAddr[i].SockAddr.IP6Address = DnsServers[i].GetAddressBytes();
					length.DnsAddr[i].SockAddrLength = Marshal.SizeOf(typeof(sockaddr_in6));
				}
			}
			this.ServerList = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(DnsAddrArray)));
			Marshal.StructureToPtr(length, this.ServerList, false);
		}
	}
}