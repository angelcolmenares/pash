using System;

namespace Microsoft.DnsClient.Commands
{
	public enum RecordType : ushort
	{
		A_AAAA = 0,
		UNKNOWN = 0,
		A = 1,
		NS = 2,
		MD = 3,
		MF = 4,
		CNAME = 5,
		SOA = 6,
		MB = 7,
		MG = 8,
		MR = 9,
		NULL = 10,
		WKS = 11,
		PTR = 12,
		HINFO = 13,
		MINFO = 14,
		MX = 15,
		TXT = 16,
		RP = 17,
		AFSDB = 18,
		X25 = 19,
		ISDN = 20,
		RT = 21,
		AAAA = 28,
		SRV = 33,
		DNAME = 39,
		OPT = 41,
		DS = 43,
		RRSIG = 46,
		NSEC = 47,
		DNSKEY = 48,
		DHCID = 49,
		NSEC3 = 50,
		NSEC3PARAM = 51,
		ANY = 255,
		ALL = 255,
		WINS = 65281
	}
}