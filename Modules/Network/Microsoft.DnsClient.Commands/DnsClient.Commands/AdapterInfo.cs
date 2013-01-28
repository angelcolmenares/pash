using System;
using System.Collections.Generic;

namespace Microsoft.DnsClient.Commands
{
	public struct AdapterInfo
	{
		public string FriendlyName;

		public string Description;

		public string DNSSuffix;

		public List<string> Ipv4;

		public List<string> Ipv6;

		public InterfaceType IfType;

	}
}