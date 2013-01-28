using System;

namespace Microsoft.PowerShell.Workflow
{
	internal class CloseOperation : ThrottleOperation
	{
		private readonly Connection _connection;

		internal Connection Connection
		{
			get
			{
				return this._connection;
			}
		}

		internal CloseOperation(Connection connection)
		{
			this._connection = connection;
			this._connection.CloseCompleted += new EventHandler(this.HandleCloseCompleted);
		}

		internal override void DoOperation()
		{
			this._connection.CloseAsync();
		}

		private void HandleCloseCompleted(object sender, EventArgs eventArgs)
		{
			this._connection.CloseCompleted -= new EventHandler(this.HandleCloseCompleted);
			base.RaiseOperationComplete();
		}
	}
}