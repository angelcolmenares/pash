using System;

namespace System.DirectoryServices.AccountManagement
{
	internal class ConfigSettings
	{
		private DebugLevel debugLevel;

		private string debugLogFile;

		public DebugLevel DebugLevel
		{
			get
			{
				return this.debugLevel;
			}
		}

		public string DebugLogFile
		{
			get
			{
				return this.debugLogFile;
			}
		}

		public ConfigSettings(DebugLevel debugLevel, string debugLogFile)
		{
			this.debugLevel = debugLevel;
			this.debugLogFile = debugLogFile;
		}

		public ConfigSettings() : this(0, null)
		{
		}
	}
}