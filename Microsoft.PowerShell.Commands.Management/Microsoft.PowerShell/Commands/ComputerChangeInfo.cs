using System;
using System.Management.Automation.Internal;

namespace Microsoft.PowerShell.Commands
{
	public sealed class ComputerChangeInfo
	{
		private const string MatchFormat = "{0}:{1}";

		private bool hasSucceeded;

		private string computername;

		public string ComputerName
		{
			get
			{
				return this.computername;
			}
			set
			{
				this.computername = value;
			}
		}

		public bool HasSucceeded
		{
			get
			{
				return this.hasSucceeded;
			}
			set
			{
				this.hasSucceeded = value;
			}
		}

		public ComputerChangeInfo()
		{
		}

		private string FormatLine(string HasSucceeded, string computername)
		{
			return StringUtil.Format("{0}:{1}", HasSucceeded, computername);
		}

		public override string ToString()
		{
			bool hasSucceeded = this.HasSucceeded;
			return this.FormatLine(hasSucceeded.ToString(), this.computername);
		}
	}
}