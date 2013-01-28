using System;
using System.Management.Automation.Host;

namespace Microsoft.Management.PowerShellWebAccess.Primitives
{
	public class ClientConfiguration
	{
		public string BackgroundColor
		{
			get;
			set;
		}

		[CLSCompliant(false)]
		public Size BufferSize
		{
			get;
			set;
		}

		public string ComputerName
		{
			get;
			set;
		}

		public string ErrorBackgroundColor
		{
			get;
			set;
		}

		public string ErrorForegroundColor
		{
			get;
			set;
		}

		public string ForegroundColor
		{
			get;
			set;
		}

		public string InputBackgroundColor
		{
			get;
			set;
		}

		public string InputForegroundColor
		{
			get;
			set;
		}

		public string Prompt
		{
			get;
			set;
		}

		public int SessionTimeout
		{
			get;
			set;
		}

		public int SessionTimeoutWarning
		{
			get;
			set;
		}

		public ClientMessage[] StartupMessages
		{
			get;
			set;
		}

		public string WarningBackgroundColor
		{
			get;
			set;
		}

		public string WarningForegroundColor
		{
			get;
			set;
		}

		[CLSCompliant(false)]
		public Size WindowSize
		{
			get;
			set;
		}

		public string WindowTitle
		{
			get;
			set;
		}

		public ClientConfiguration()
		{
		}
	}
}