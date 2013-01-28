using System;
using System.Configuration;

namespace System.DirectoryServices.AccountManagement
{
	internal static class GlobalConfig
	{
		public const DebugLevel DefaultDebugLevel = DebugLevel.None;

		private static ConfigSettings configSettings;

		public static DebugLevel DebugLevel
		{
			get
			{
				if (GlobalConfig.configSettings != null)
				{
					return GlobalConfig.configSettings.DebugLevel;
				}
				else
				{
					return DebugLevel.None;
				}
			}
		}

		public static string DebugLogFile
		{
			get
			{
				if (GlobalConfig.configSettings != null)
				{
					return GlobalConfig.configSettings.DebugLogFile;
				}
				else
				{
					return null;
				}
			}
		}

		static GlobalConfig()
		{
			GlobalConfig.configSettings = (ConfigSettings)ConfigurationManager.GetSection("System.DirectoryServices.AccountManagement");
		}
	}
}