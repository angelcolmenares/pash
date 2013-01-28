using System;

namespace Microsoft.PowerShell.Activities
{
	public class HostSettingCommandMetadata
	{
		public string CommandName
		{
			get;
			set;
		}

		public int EndColumnNumber
		{
			get;
			set;
		}

		public int EndLineNumber
		{
			get;
			set;
		}

		public int StartColumnNumber
		{
			get;
			set;
		}

		public int StartLineNumber
		{
			get;
			set;
		}

		public HostSettingCommandMetadata()
		{
		}
	}
}