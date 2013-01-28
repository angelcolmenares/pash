using System;
using System.Runtime.Serialization;

namespace System.Activities
{
	[Serializable]
	public class WorkflowApplicationCompletedException : WorkflowApplicationException
	{
		public WorkflowApplicationCompletedException () : this ("Workflow application has completed") {}
		public WorkflowApplicationCompletedException (string msg) : base (msg) {}
		public WorkflowApplicationCompletedException (string msg, Exception inner) : base (msg, inner) {}
		protected WorkflowApplicationCompletedException (SerializationInfo info, StreamingContext context) :
			base (info, context) {}
	}
}
