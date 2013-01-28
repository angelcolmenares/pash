using System;
using System.Runtime.InteropServices;

namespace System.DirectoryServices.Protocols
{
	internal class Utility
	{
		private static bool platformSupported;

		private static bool isWin2kOS;

		private static bool isWin2k3Above;

		internal static bool IsWin2k3AboveOS
		{
			get
			{
				return Utility.isWin2k3Above;
			}
		}

		internal static bool IsWin2kOS
		{
			get
			{
				return Utility.isWin2kOS;
			}
		}

		static Utility ()
		{
			OperatingSystem oSVersion = Environment.OSVersion;
			if (oSVersion.Platform == PlatformID.MacOSX || oSVersion.Platform == PlatformID.Unix) {
				Utility.platformSupported = true;
				Utility.isWin2k3Above = true;
			}
			else if (oSVersion.Platform == PlatformID.Win32NT && oSVersion.Version.Major >= 5)
			{
				Utility.platformSupported = true;
				if (oSVersion.Version.Major == 5 && oSVersion.Version.Minor == 0)
				{
					Utility.isWin2kOS = true;
				}
				if (oSVersion.Version.Major > 5 || oSVersion.Version.Minor >= 2)
				{
					Utility.isWin2k3Above = true;
				}
			}
		}

		public Utility()
		{
		}

		internal static IntPtr AllocHGlobalIntPtrArray(int size)
		{
			IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(IntPtr)) * size);
			for (int i = 0; i < size; i++)
			{
				IntPtr intPtr1 = (IntPtr)((long)intPtr + (long)(Marshal.SizeOf(typeof(IntPtr)) * i));
				Marshal.WriteIntPtr(intPtr1, IntPtr.Zero);
			}
			return intPtr;
		}

		internal static void CheckOSVersion()
		{
			if (Utility.platformSupported)
			{
				return;
			}
			else
			{
				throw new PlatformNotSupportedException(Res.GetString("SupportedPlatforms"));
			}
		}

		internal static bool IsLdapError(LdapError error)
		{
			if (error == LdapError.IsLeaf || error == LdapError.InvalidCredentials || error == LdapError.SendTimeOut)
			{
				return true;
			}
			else
			{
				if (error < LdapError.ServerDown || error > LdapError.ReferralLimitExceeded)
				{
					return false;
				}
				else
				{
					return true;
				}
			}
		}

		internal static bool IsResultCode(ResultCode code)
		{
			if (code < ResultCode.Success || code > ResultCode.SaslBindInProgress)
			{
				if (code < ResultCode.NoSuchAttribute || code > ResultCode.InvalidAttributeSyntax)
				{
					if (code < ResultCode.NoSuchObject || code > ResultCode.InvalidDNSyntax)
					{
						if (code < ResultCode.InsufficientAccessRights || code > ResultCode.LoopDetect)
						{
							if (code < ResultCode.NamingViolation || code > ResultCode.AffectsMultipleDsas)
							{
								if (code == ResultCode.AliasDereferencingProblem || code == ResultCode.InappropriateAuthentication || code == ResultCode.SortControlMissing || code == ResultCode.OffsetRangeError || code == ResultCode.VirtualListViewError || code == ResultCode.Other)
								{
									return true;
								}
								else
								{
									return false;
								}
							}
							else
							{
								return true;
							}
						}
						else
						{
							return true;
						}
					}
					else
					{
						return true;
					}
				}
				else
				{
					return true;
				}
			}
			else
			{
				return true;
			}
		}
	}
}