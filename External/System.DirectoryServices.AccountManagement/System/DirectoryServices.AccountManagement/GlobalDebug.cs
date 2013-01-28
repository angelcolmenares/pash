using System;
using System.Diagnostics;
using System.Globalization;
using System.Security;

namespace System.DirectoryServices.AccountManagement
{
	internal static class GlobalDebug
	{
		private static DebugLevel debugLevel;

		public static bool Error
		{
			get
			{
				return DebugLevel.Error >= GlobalDebug.debugLevel;
			}
		}

		public static bool Info
		{
			get
			{
				return DebugLevel.Info >= GlobalDebug.debugLevel;
			}
		}

		public static bool Warn
		{
			get
			{
				return DebugLevel.Warn >= GlobalDebug.debugLevel;
			}
		}

		[SecurityCritical]
		static GlobalDebug()
		{
			GlobalDebug.debugLevel = GlobalConfig.DebugLevel;
		}

		[Conditional("DEBUG")]
		[SecuritySafeCritical]
		public static void WriteLineIf(bool f, string category, string message, object[] args)
		{
			int currentThreadId = SafeNativeMethods.GetCurrentThreadId();
			message = string.Concat("[", currentThreadId.ToString("x", CultureInfo.InvariantCulture), "] ", message);
		}

		[Conditional("DEBUG")]
		[SecuritySafeCritical]
		public static void WriteLineIf(bool f, string category, string message)
		{
			int currentThreadId = SafeNativeMethods.GetCurrentThreadId();
			message = string.Concat("[", currentThreadId.ToString("x", CultureInfo.InvariantCulture), "] ", message);
		}
	}
}