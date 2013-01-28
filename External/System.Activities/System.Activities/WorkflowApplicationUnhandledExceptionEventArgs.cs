using System;
using System.Runtime.Serialization;

namespace System.Activities
{
	public class WorkflowApplicationUnhandledExceptionEventArgs : WorkflowApplicationEventArgs
	{
		internal WorkflowApplicationUnhandledExceptionEventArgs ()
		{
		}

		public Activity ExceptionSource { get; private set; }
		public string ExceptionSourceInstanceId { get; private set; }
		public Exception UnhandledException { get; private set; }
	}
}
