using System;

namespace System.DirectoryServices.AccountManagement
{
	internal struct ServerProperties
	{
		public string dnsHostName;

		public DomainControllerMode OsVersion;

		public ContextType contextType;

		public string[] SupportCapabilities;

		public int portSSL;

		public int portLDAP;

	}
}