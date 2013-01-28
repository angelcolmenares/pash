using System;

namespace System.DirectoryServices.ActiveDirectory
{
	internal sealed class DomainControllerInfo
	{
		public string DomainControllerName;

		public string DomainControllerAddress;

		public int DomainControllerAddressType;

		public Guid DomainGuid;

		public string DomainName;

		public string DnsForestName;

		public int Flags;

		public string DcSiteName;

		public string ClientSiteName;

		public DomainControllerInfo()
		{
		}
	}
}