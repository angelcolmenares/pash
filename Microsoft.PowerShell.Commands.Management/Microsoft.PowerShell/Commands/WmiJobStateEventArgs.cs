using System;

namespace Microsoft.PowerShell.Commands
{
	internal sealed class WmiJobStateEventArgs : EventArgs
	{
		private WmiState wmiState;

		internal WmiState WmiState
		{
			get
			{
				return this.wmiState;
			}
			set
			{
				this.wmiState = value;
			}
		}

		public WmiJobStateEventArgs()
		{
		}
	}
}