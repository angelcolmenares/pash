using System;

namespace System.DirectoryServices.AccountManagement
{
	internal class PropertyNamesExternal
	{
		private static int AcctInfoPrefixLength;

		private static int PwdInfoPrefixLength;

		static PropertyNamesExternal()
		{
			PropertyNamesExternal.AcctInfoPrefixLength = "AuthenticablePrincipal.AccountInfo".Length;
			PropertyNamesExternal.PwdInfoPrefixLength = "AuthenticablePrincipal.PasswordInfo".Length;
		}

		private PropertyNamesExternal()
		{
		}

		internal static string GetExternalForm(string propertyName)
		{
			if (!propertyName.StartsWith("AuthenticablePrincipal.AccountInfo", StringComparison.Ordinal))
			{
				if (!propertyName.StartsWith("AuthenticablePrincipal.PasswordInfo", StringComparison.Ordinal))
				{
					return propertyName;
				}
				else
				{
					return string.Concat("AuthenticablePrincipal", propertyName.Substring(PropertyNamesExternal.PwdInfoPrefixLength));
				}
			}
			else
			{
				return string.Concat("AuthenticablePrincipal", propertyName.Substring(PropertyNamesExternal.AcctInfoPrefixLength));
			}
		}
	}
}