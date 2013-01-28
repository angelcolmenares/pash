using System;
using System.Management.Automation.Internal;

namespace Microsoft.PowerShell.Commands
{
	public sealed class RenameComputerChangeInfo
	{
		private const string MatchFormat = "{0}:{1}:{2}";

		private bool hasSucceeded;

		private string newcomputername;

		private string oldcomputername;

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

		public string NewComputerName
		{
			get
			{
				return this.newcomputername;
			}
			set
			{
				this.newcomputername = value;
			}
		}

		public string OldComputerName
		{
			get
			{
				return this.oldcomputername;
			}
			set
			{
				this.oldcomputername = value;
			}
		}

		public RenameComputerChangeInfo()
		{
		}

		private string FormatLine(string HasSucceeded, string newcomputername, string oldcomputername)
		{
			object[] hasSucceeded = new object[3];
			hasSucceeded[0] = HasSucceeded;
			hasSucceeded[1] = newcomputername;
			hasSucceeded[2] = oldcomputername;
			return StringUtil.Format("{0}:{1}:{2}", hasSucceeded);
		}

		public override string ToString()
		{
			bool hasSucceeded = this.HasSucceeded;
			return this.FormatLine(hasSucceeded.ToString(), this.newcomputername, this.oldcomputername);
		}
	}
}