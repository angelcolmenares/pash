using System;
using System.Collections.Generic;

namespace System.Activities
{
	public class WorkflowApplicationCompletedEventArgs : WorkflowApplicationEventArgs
	{
		internal WorkflowApplicationCompletedEventArgs ()
		{
		}

		public ActivityInstanceState CompletionState { get { throw new NotImplementedException (); } }
		public IDictionary<string, object> Outputs { get { throw new NotImplementedException (); } }
		public Exception TerminationException { get { throw new NotImplementedException (); } }
	}
}
