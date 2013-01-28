using System;

namespace Microsoft.DnsClient.Commands
{
	internal enum FAMILY : uint
	{
		AF_UNSPEC = 0,
		AF_INET = 2,
		AF_INET6 = 23
	}
}