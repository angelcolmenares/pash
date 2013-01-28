using System;

namespace Microsoft.PowerShell.Workflow
{
	internal class ActivityHostCrashedEventArgs : EventArgs
	{
		internal bool FailureOnSetup
		{
			get;
			set;
		}

		internal ActivityInvoker Invoker
		{
			get;
			set;
		}

		public ActivityHostCrashedEventArgs()
		{
		}
	}
}