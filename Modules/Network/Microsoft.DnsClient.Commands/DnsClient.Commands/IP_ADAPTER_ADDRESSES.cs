using System;

namespace Microsoft.DnsClient.Commands
{
	internal struct IP_ADAPTER_ADDRESSES
	{
		public ulong Alignment;

		public IntPtr Next;

		public IntPtr AdapterName;

		public IntPtr FirstUnicastAddress;

		public IntPtr FirstAnycastAddress;

		public IntPtr FirstMulticastAddress;

		public IntPtr FirstDnsServerAddress;

		public string DnsSuffix;

		public string Description;

		public string FriendlyName;

		public byte[] PhysicalAddress;

		public uint PhysicalAddressLength;

		public uint Flags;

		public uint Mtu;

		public uint IfType;

		public uint OperStatus;

		private uint Ipv6IfIndex;

		public uint[] ZoneIndices;

		public IntPtr FirstPrefix;

		public ulong TransmitLinkSpeed;

		public ulong ReceiveLinkSpeed;

		public IntPtr FirstWinsServerAddress;

		public IntPtr FirstGatewayAddress;

		public ulong Ipv4Metric;

		public ulong Ipv6Metric;

		public ulong Luid;

		public SOCKET_ADDRESS Dhcpv4Server;

		public uint CompartmentId;

		public GUID NetworkGuid;

		public uint ConnectionType;

		public uint TunnelType;

		public SOCKET_ADDRESS Dhcpv6Server;

		public byte[] Dhcpv6ClientDuid;

		public ulong Dhcpv6ClientDuidLength;

		public ulong Dhcpv6Iaid;

		public IntPtr FirstDnsSuffix;

	}
}