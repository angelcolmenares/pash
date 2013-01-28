using System;

namespace Microsoft.PowerShell.Workflow
{
	internal class DisconnectOperation : ThrottleOperation
	{
		private readonly Connection _connection;

		internal DisconnectOperation(Connection connection)
		{
			this._connection = connection;
			this._connection.DisconnectCompleted += new EventHandler(this.HandleDisconnectCompleted);
		}

		internal override void DoOperation()
		{
			this._connection.DisconnectAsync();
		}

		private void HandleDisconnectCompleted(object sender, EventArgs eventArgs)
		{
			this._connection.DisconnectCompleted -= new EventHandler(this.HandleDisconnectCompleted);
			base.RaiseOperationComplete();
		}
	}
}