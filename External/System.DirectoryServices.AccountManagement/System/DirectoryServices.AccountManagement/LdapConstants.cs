using System;

namespace System.DirectoryServices.AccountManagement
{
	internal class LdapConstants
	{
		public static int LDAP_SSL_PORT;

		public static int LDAP_PORT;

		internal static DateTime defaultUtcTime;

		static LdapConstants()
		{
			LdapConstants.LDAP_SSL_PORT = 0x27c;
			LdapConstants.LDAP_PORT = 0x185;
			LdapConstants.defaultUtcTime = new DateTime(0x641, 1, 1, 0, 0, 0);
		}

		private LdapConstants()
		{
		}
	}
}