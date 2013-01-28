using System;
using System.Runtime.Serialization;

namespace System.Activities
{
	[Serializable]
	public class WorkflowApplicationTerminatedException : WorkflowApplicationCompletedException
	{
		public WorkflowApplicationTerminatedException () : this ("Workflow application has been terminated") {}
		public WorkflowApplicationTerminatedException (string msg) : base (msg) {}
		public WorkflowApplicationTerminatedException (string msg, Exception inner) : base (msg, inner) {}
		protected WorkflowApplicationTerminatedException (SerializationInfo info, StreamingContext context) :
			base (info, context) {}
	}
}
