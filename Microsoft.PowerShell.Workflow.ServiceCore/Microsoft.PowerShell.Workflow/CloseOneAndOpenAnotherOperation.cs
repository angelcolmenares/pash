using System;

namespace Microsoft.PowerShell.Workflow
{
	internal class CloseOneAndOpenAnotherOperation : ThrottleOperation
	{
		private readonly Connection _connectionToClose;

		private readonly Connection _connectionToOpen;

		internal CloseOneAndOpenAnotherOperation(Connection toClose, Connection toOpen)
		{
			this._connectionToClose = toClose;
			this._connectionToOpen = toOpen;
			this._connectionToClose.CloseCompleted += new EventHandler(this.HandleCloseCompleted);
			this._connectionToOpen.OpenCompleted += new EventHandler(this.HandleOpenCompleted);
		}

		internal override void DoOperation()
		{
			this._connectionToClose.CloseAsync();
		}

		private void HandleCloseCompleted(object sender, EventArgs eventArgs)
		{
			this._connectionToClose.CloseCompleted -= new EventHandler(this.HandleCloseCompleted);
			this._connectionToOpen.OpenAsync();
		}

		private void HandleOpenCompleted(object sender, EventArgs eventArgs)
		{
			this._connectionToOpen.OpenCompleted -= new EventHandler(this.HandleOpenCompleted);
			base.RaiseOperationComplete();
		}
	}
}