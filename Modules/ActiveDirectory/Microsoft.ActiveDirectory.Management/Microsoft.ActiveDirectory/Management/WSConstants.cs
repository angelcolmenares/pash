using System;

namespace Microsoft.ActiveDirectory.Management
{
	internal class WSConstants
	{
		internal const int maxBufferInBytes = 0x500000;

		internal const int ErrorCodeLengthInExtendedError = 8;

		internal const string WcfNetTcpEndpointPrefix = "net.tcp://";

		internal const string WcfAdwsEndpointPrefix = ":9389/ActiveDirectoryWebServices/";

		internal const string WcfWindowsEndpointPrefix = "Windows/";

		internal const string WcfUserNameEndpointPrefix = "UserName/";

		internal const string WcfResourceEndpoint = "Resource";

		internal const string WcfResourceFactoryEndpoint = "ResourceFactory";

		internal const string WcfEnumEndpoint = "Enumeration";

		internal const string WcfMexEndpoint = "mex";

		internal const string WcfAcctMgmtEndpoint = "AccountManagement";

		internal const string WcfTopoMgmtEndpoint = "TopologyManagement";

		internal const string LdapInstancePrefix = "ldap:";

		internal const string RootDSEGuidString = "11111111-1111-1111-1111-111111111111";

		internal static TimeSpan DefaultWcfSendTimeout;

		internal readonly static TimeSpan DefaultAccountManagementCATimeout;

		static WSConstants()
		{
			WSConstants.DefaultWcfSendTimeout = new TimeSpan(0, 2, 0);
			WSConstants.DefaultAccountManagementCATimeout = new TimeSpan(0, 5, 0);
		}

		private WSConstants()
		{
		}
	}
}