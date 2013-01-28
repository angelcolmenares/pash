namespace Microsoft.DnsClient.Commands
{
	internal enum DnsConfigType
	{
		DnsConfigPrimaryDomainName_W = 0,
		DnsConfigPrimaryDomainName_A = 1,
		DnsConfigPrimaryDomainName_UTF8 = 2,
		DnsConfigAdapterDomainName_W = 3,
		DnsConfigAdapterDomainName_A = 4,
		DnsConfigAdapterDomainName_UTF8 = 5,
		DnsConfigDnsServerList = 6,
		DnsConfigSearchList = 7,
		DnsConfigAdapterInfo = 8,
		DnsConfigPrimaryHostNameRegistrationEnabled = 9,
		DnsConfigAdapterHostNameRegistrationEnabled = 10,
		DnsConfigAddressRegistrationMaxCount = 11,
		DnsConfigHostName_W = 12,
		DnsConfigHostName_A = 13,
		DnsConfigHostName_UTF8 = 14,
		DnsConfigFullHostName_W = 15,
		DnsConfigFullHostName_A = 16,
		DnsConfigFullHostName_UTF8 = 17,
		DnsConfigSearchInformation = 65539
	}
}