using System;
using System.Runtime.InteropServices;

namespace Microsoft.DnsClient.Commands
{
	[StructLayout(LayoutKind.Explicit)]
	internal struct DnsAddr
	{
		[FieldOffset(0)]
		internal sockaddr_in6 SockAddr;

		[FieldOffset(32)]
		internal int SockAddrLength;

		[FieldOffset(36)]
		internal int SubnetLength;

		[FieldOffset(40)]
		internal uint Flags;

		[FieldOffset(44)]
		internal uint Status;

		[FieldOffset(48)]
		internal uint Priority;

		[FieldOffset(52)]
		internal uint Weight;

		[FieldOffset(56)]
		internal uint Tag;

		[FieldOffset(60)]
		internal uint PayloadSize;

	}
}