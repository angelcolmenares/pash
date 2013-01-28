using System;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	internal sealed class CmdletActionEventArgs : EventArgs
	{
		public readonly CimBaseAction Action;

		public CmdletActionEventArgs(CimBaseAction action)
		{
			this.Action = action;
		}
	}
}