using System;

namespace Microsoft.ActiveDirectory.Management
{
	internal struct DOMAIN_CONTROLLER_INFO
	{
		public string DomainControllerName;

		public string DomainControllerAddress;

		public uint DomainControllerAddressType;

		public Guid DomainGuid;

		public string DomainName;

		public string DnsForestName;

		public uint Flags;

		public string DcSiteName;

		public string ClientSiteName;

	}
}