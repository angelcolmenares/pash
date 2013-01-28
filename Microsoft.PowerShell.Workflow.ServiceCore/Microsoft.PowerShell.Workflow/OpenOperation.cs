using System;

namespace Microsoft.PowerShell.Workflow
{
	internal class OpenOperation : ThrottleOperation
	{
		private readonly Connection _connection;

		internal OpenOperation(Connection connection)
		{
			this._connection = connection;
			this._connection.OpenCompleted += new EventHandler(this.HandleOpenCompleted);
		}

		internal override void DoOperation()
		{
			this._connection.OpenAsync();
		}

		private void HandleOpenCompleted(object sender, EventArgs eventArgs)
		{
			this._connection.OpenCompleted -= new EventHandler(this.HandleOpenCompleted);
			base.RaiseOperationComplete();
		}
	}
}