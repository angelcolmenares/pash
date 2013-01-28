using System;
using System.Configuration;

namespace Microsoft.ActiveDirectory.Management
{
	public class TraceElement : ConfigurationElement
	{
		internal const string LevelAttribute = "level";

		internal const string LogFileAttribute = "logFile";

		private DebugLogLevel? _level;

		[ConfigurationProperty("level", DefaultValue=null, IsRequired=false)]
		[StringValidator(MinLength=0, MaxLength=10)]
		public string Level
		{
			get
			{
				return (string)base["level"];
			}
			set
			{
				base["level"] = value;
			}
		}

		[ConfigurationProperty("logFile", DefaultValue=null, IsRequired=false)]
		[StringValidator(InvalidCharacters="*/|", MinLength=0, MaxLength=60)]
		public string LogFile
		{
			get
			{
				return (string)base["logFile"];
			}
			set
			{
				base["logFile"] = value;
			}
		}

		internal DebugLogLevel LogLevel
		{
			get
			{
				if (!this._level.HasValue)
				{
					this._level = new DebugLogLevel?(DebugLogLevel.Off);
					if (!string.IsNullOrEmpty(this.Level))
					{
						this._level = new DebugLogLevel?((DebugLogLevel)Enum.Parse(typeof(DebugLogLevel), this.Level, true));
					}
				}
				return this._level.Value;
			}
		}

		public TraceElement()
		{
			this._level = null;
		}
	}
}