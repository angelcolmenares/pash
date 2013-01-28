using System;

namespace Microsoft.PowerShell.Workflow
{
	internal abstract class ThrottleOperation
	{
		private readonly static EventArgs EventArgs;

		static ThrottleOperation()
		{
			ThrottleOperation.EventArgs = new EventArgs();
		}

		protected ThrottleOperation()
		{
		}

		internal virtual void DoOperation()
		{
			throw new NotImplementedException();
		}

		internal void RaiseOperationComplete()
		{
			if (this.OperationComplete != null)
			{
				this.OperationComplete(this, ThrottleOperation.EventArgs);
			}
		}

		internal event EventHandler OperationComplete;
	}
}