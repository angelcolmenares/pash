using System;

namespace Microsoft.PowerShell.Workflow
{
	internal class WorkflowStatusEventArgs : EventArgs
	{
		private Guid id;

		private WorkflowInstanceState state;

		private Exception unhandledException;

		internal Guid Id
		{
			get
			{
				return this.id;
			}
		}

		internal WorkflowInstanceState State
		{
			get
			{
				return this.state;
			}
		}

		internal Exception UnhandledException
		{
			get
			{
				return this.unhandledException;
			}
		}

		internal WorkflowStatusEventArgs(Guid id, WorkflowInstanceState state, Exception unhandledException)
		{
			this.id = id;
			this.state = state;
			this.unhandledException = unhandledException;
		}
	}
}