using System;

namespace Microsoft.DnsClient.Commands
{
	internal enum DNS_FREE_TYPE : uint
	{
		DnsFreeFlat,
		DnsFreeRecordList,
		DnsFreeParsedMessageFields
	}
}