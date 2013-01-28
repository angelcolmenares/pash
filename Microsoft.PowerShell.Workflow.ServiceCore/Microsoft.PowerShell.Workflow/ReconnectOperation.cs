using System;
using System.Management.Automation.Runspaces;

namespace Microsoft.PowerShell.Workflow
{
	internal class ReconnectOperation : ThrottleOperation
	{
		private readonly Connection _connection;

		internal ReconnectOperation(Connection connection)
		{
			this._connection = connection;
			this._connection.ReconnectCompleted += new EventHandler(this.HandleReconnectCompleted);
		}

		internal override void DoOperation()
		{
			ReconnectOperation eventHandler = null;
			if (this._connection.Runspace.RunspaceStateInfo.State == RunspaceState.Disconnected)
			{
				this._connection.ReconnectAsync();
				return;
			}
			else
			{
				EventHandler<RunspaceStateEventArgs> eventHandler1 = (object sender, RunspaceStateEventArgs e) => {
					if (e.RunspaceStateInfo.State == RunspaceState.Disconnected)
					{
						this._connection.ReconnectAsync();
						this._connection.Runspace.StateChanged -= this.HandleRunspaceStateChanged;
					}
				};
				eventHandler.HandleRunspaceStateChanged = null;
				eventHandler.HandleRunspaceStateChanged = (object sender, RunspaceStateEventArgs e) => {
					if (e.RunspaceStateInfo.State == RunspaceState.Disconnected)
					{
						this._connection.ReconnectAsync();
						this._connection.Runspace.StateChanged -= this.HandleRunspaceStateChanged;
					}
				};
				this._connection.Runspace.StateChanged += eventHandler.HandleRunspaceStateChanged;
				if (this._connection.Runspace.RunspaceStateInfo.State == RunspaceState.Disconnected)
				{
					this._connection.ReconnectAsync();
					this._connection.Runspace.StateChanged -= eventHandler.HandleRunspaceStateChanged;
				}
				return;
			}
		}

		protected virtual void HandleRunspaceStateChanged (object sender, RunspaceStateEventArgs e)
		{

		}

		protected virtual void HandleReconnectCompleted(object sender, EventArgs eventArgs)
		{
			this._connection.ReconnectCompleted -= new EventHandler(this.HandleReconnectCompleted);
			base.RaiseOperationComplete();
		}
	}
}