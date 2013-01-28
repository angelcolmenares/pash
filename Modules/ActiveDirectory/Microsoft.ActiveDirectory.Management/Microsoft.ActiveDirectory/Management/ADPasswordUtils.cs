using System;
using System.Management.Automation;
using System.Runtime.InteropServices;
using System.Security;

namespace Microsoft.ActiveDirectory.Management
{
	internal class ADPasswordUtils
	{
		private static string _debugCategory;

		static ADPasswordUtils()
		{
			ADPasswordUtils._debugCategory = "ADPasswordUtils";
		}

		public ADPasswordUtils()
		{
		}

		public static bool AreCredentialsExplicit(PSCredential creds)
		{
			if (creds != null)
			{
				if (creds.UserName == null)
				{
					return false;
				}
				else
				{
					return null != creds.Password;
				}
			}
			else
			{
				return false;
			}
		}

		public static bool MatchCredentials(PSCredential cacheCred, PSCredential userCred)
		{
			if (!ADPasswordUtils.MatchPassword(cacheCred.Password, userCred.Password))
			{
				return false;
			}
			else
			{
				return string.Equals(cacheCred.UserName, userCred.UserName, StringComparison.OrdinalIgnoreCase);
			}
		}

		public static bool MatchPassword(SecureString s1, SecureString s2)
		{
			byte num;
			byte num1;
			bool flag;
			if (s1.Length == s2.Length)
			{
				IntPtr zero = IntPtr.Zero;
				IntPtr bSTR = IntPtr.Zero;
				try
				{
					zero = Marshal.SecureStringToBSTR(s1);
					bSTR = Marshal.SecureStringToBSTR(s2);
					DebugLogger.LogInfo(ADPasswordUtils._debugCategory, "MatchPassword: Matching passwords");
					int num2 = 0;
					do
					{
						num1 = Marshal.ReadByte(zero, num2 + 1);
						byte num3 = Marshal.ReadByte(bSTR, num2 + 1);
						num = Marshal.ReadByte(zero, num2);
						byte num4 = Marshal.ReadByte(bSTR, num2);
						num2 = num2 + 2;
						if (num1 == num3 && num == num4)
						{
							continue;
						}
						flag = false;
						return flag;
					}
					while (num1 != 0 || num != 0);
					DebugLogger.LogInfo(ADPasswordUtils._debugCategory, "MatchPassword: found match");
					flag = true;
				}
				finally
				{
					if (IntPtr.Zero != zero)
					{
						Marshal.ZeroFreeBSTR(zero);
					}
					if (IntPtr.Zero != bSTR)
					{
						Marshal.ZeroFreeBSTR(bSTR);
					}
				}
				return flag;
			}
			else
			{
				return false;
			}
		}
	}
}