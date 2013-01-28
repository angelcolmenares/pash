using System;
using System.Collections.Generic;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	internal static class PasswordPropertiesUtil
	{
		internal const int PASSWORD_COMPLEX = 1;

		internal const int PASSWORD_NO_ANON_CHANGE = 2;

		internal const int PASSWORD_NO_CLEAR_CHANGE = 4;

		internal const int LOCKOUT_ADMINS = 8;

		internal const int PASSWORD_STORE_CLEARTEXT = 16;

		internal const int REFUSE_PASSWORD_CHANGE = 32;

		private static Dictionary<string, int> stringToBit;

		static PasswordPropertiesUtil()
		{
			PasswordPropertiesUtil.stringToBit = new Dictionary<string, int>();
			PasswordPropertiesUtil.stringToBit.Add("ComplexityEnabled", 1);
			PasswordPropertiesUtil.stringToBit.Add("ReversibleEncryptionEnabled", 16);
		}

		internal static bool IsInverseBit(int bit)
		{
			return false;
		}

		internal static int StringToBit(string attribute)
		{
			int item;
			try
			{
				item = PasswordPropertiesUtil.stringToBit[attribute];
			}
			catch
			{
				item = 0;
			}
			return item;
		}
	}
}