using System;
using System.Collections.Generic;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	internal class UserAccountControlUtil
	{
		internal const int SCRIPT = 1;

		internal const int ACCOUNTDISABLE = 2;

		internal const int HOMEDIR_REQUIRED = 8;

		internal const int LOCKOUT = 16;

		internal const int PASSWD_NOTREQD = 32;

		internal const int PASSWD_CANT_CHANGE = 64;

		internal const int ENCRYPTED_TEXT_PWD_ALLOWED = 128;

		internal const int TEMP_DUPLICATE_ACCOUNT = 0x100;

		internal const int NORMAL_ACCOUNT = 0x200;

		internal const int INTERDOMAIN_TRUST_ACCOUNT = 0x800;

		internal const int WORKSTATION_TRUST_ACCOUNT = 0x1000;

		internal const int SERVER_TRUST_ACCOUNT = 0x2000;

		internal const int DONT_EXPIRE_PASSWORD = 0x10000;

		internal const int MNS_LOGON_ACCOUNT = 0x20000;

		internal const int SMARTCARD_REQUIRED = 0x40000;

		internal const int TRUSTED_FOR_DELEGATION = 0x80000;

		internal const int NOT_DELEGATED = 0x100000;

		internal const int USE_DES_KEY_ONLY = 0x200000;

		internal const int DONT_REQUIRE_PREAUTH = 0x400000;

		internal const int PASSWORD_EXPIRED = 0x800000;

		internal const int TRUSTED_TO_AUTHENTICATE_FOR_DELEGATION = 0x1000000;

		private static Dictionary<string, int> stringToBit;

		static UserAccountControlUtil()
		{
			UserAccountControlUtil.stringToBit = new Dictionary<string, int>();
			UserAccountControlUtil.stringToBit.Add("AccountNotDelegated", 0x100000);
			UserAccountControlUtil.stringToBit.Add("Enabled", 2);
			UserAccountControlUtil.stringToBit.Add("LockedOut", 16);
			UserAccountControlUtil.stringToBit.Add("PasswordNeverExpires", 0x10000);
			UserAccountControlUtil.stringToBit.Add("PasswordNotRequired", 32);
			UserAccountControlUtil.stringToBit.Add("TrustedForDelegation", 0x80000);
			UserAccountControlUtil.stringToBit.Add("CannotChangePassword", 64);
			UserAccountControlUtil.stringToBit.Add("AllowReversiblePasswordEncryption", 128);
			UserAccountControlUtil.stringToBit.Add("SmartcardLogonRequired", 0x40000);
			UserAccountControlUtil.stringToBit.Add("MNSLogonAccount", 0x20000);
			UserAccountControlUtil.stringToBit.Add("DoesNotRequirePreAuth", 0x400000);
			UserAccountControlUtil.stringToBit.Add("PasswordExpired", 0x800000);
			UserAccountControlUtil.stringToBit.Add("TrustedToAuthForDelegation", 0x1000000);
			UserAccountControlUtil.stringToBit.Add("HomedirRequired", 8);
			UserAccountControlUtil.stringToBit.Add("UseDESKeyOnly", 0x200000);
		}

		public UserAccountControlUtil()
		{
		}

		internal static bool IsInverseBit(int bit)
		{
			if (bit != 2)
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		internal static int StringToBit(string attribute)
		{
			int item;
			try
			{
				item = UserAccountControlUtil.stringToBit[attribute];
			}
			catch
			{
				item = 0;
			}
			return item;
		}
	}
}