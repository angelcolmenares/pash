using System;
using System.Runtime.Serialization;

namespace System.Activities
{
	[Serializable]
	public class WorkflowApplicationAbortedException : WorkflowApplicationException
	{
		public WorkflowApplicationAbortedException () : this ("Workflow application has been aborted") {}
		public WorkflowApplicationAbortedException (string msg) : base (msg) {}
		public WorkflowApplicationAbortedException (string msg, Exception inner) : base (msg, inner) {}
		protected WorkflowApplicationAbortedException (SerializationInfo info, StreamingContext context) :
			base (info, context) {}
	}
}
