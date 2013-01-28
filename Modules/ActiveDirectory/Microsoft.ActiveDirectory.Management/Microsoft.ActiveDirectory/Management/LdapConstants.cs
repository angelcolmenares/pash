using System;

namespace Microsoft.ActiveDirectory.Management
{
	internal class LdapConstants
	{
		internal const string LDAP_TRUE = "TRUE";

		internal const string LDAP_FALSE = "FALSE";

		internal const int DefaultPageSize = 0x100;

		public static int LDAP_PORT;

		public static int LDAP_SSL_PORT;

		public static int LDAP_GC_PORT;

		public static int LDAP_SSL_GC_PORT;

		internal static DateTime defaultUtcTime;

		static LdapConstants()
		{
			LdapConstants.LDAP_PORT = 0x185;
			LdapConstants.LDAP_SSL_PORT = 0x27c;
			LdapConstants.LDAP_GC_PORT = 0xcc4;
			LdapConstants.LDAP_SSL_GC_PORT = 0xcc5;
			LdapConstants.defaultUtcTime = new DateTime(0x641, 1, 1, 0, 0, 0);
		}

		private LdapConstants()
		{
		}
	}
}