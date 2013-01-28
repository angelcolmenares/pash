using Microsoft.ActiveDirectory.Management;
using System;

namespace Microsoft.ActiveDirectory.Management.Provider
{
	internal class ADProviderDefaults
	{
		public static ADPathFormat PathFormat;

		public static ADPathHostType HostType;

		public static string Server;

		public static bool IsGC;

		public static ADAuthType AuthType;

		public static bool Ssl;

		public static bool Encryption;

		public static bool Signing;

		public static int ServerSearchSizeLimit;

		public static int ServerSearchPageSize;

		public static string SearchFilter;

		public static int InternalProviderSearchPageSize;

		static ADProviderDefaults()
		{
			ADProviderDefaults.PathFormat = ADPathFormat.X500;
			ADProviderDefaults.HostType = ADPathHostType.Server;
			ADProviderDefaults.Server = null;
			ADProviderDefaults.IsGC = false;
			ADProviderDefaults.AuthType = ADAuthType.Negotiate;
			ADProviderDefaults.Ssl = false;
			ADProviderDefaults.Encryption = true;
			ADProviderDefaults.Signing = true;
			ADProviderDefaults.ServerSearchSizeLimit = 0;
			ADProviderDefaults.ServerSearchPageSize = 0x100;
			ADProviderDefaults.SearchFilter = "(objectclass=*)";
			ADProviderDefaults.InternalProviderSearchPageSize = 0x100;
		}

		public ADProviderDefaults()
		{
		}
	}
}