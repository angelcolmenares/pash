using System;

namespace System.DirectoryServices.Protocols
{
	internal enum BindMethod : uint
	{
		LDAP_AUTH_SIMPLE = 128,
		LDAP_AUTH_SASL = 131,
		LDAP_AUTH_OTHERKIND = 134,
		LDAP_AUTH_EXTERNAL = 166,
		LDAP_AUTH_SICILY = 646,
		LDAP_AUTH_NEGOTIATE = 1158,
		LDAP_AUTH_SSPI = 1158,
		LDAP_AUTH_MSN = 2182,
		LDAP_AUTH_NTLM = 4230,
		LDAP_AUTH_DPA = 8326,
		LDAP_AUTH_DIGEST = 16518
	}
}