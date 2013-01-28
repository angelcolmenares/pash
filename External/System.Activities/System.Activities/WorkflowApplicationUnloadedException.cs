using System;
using System.Runtime.Serialization;

namespace System.Activities
{
	[Serializable]
	public class WorkflowApplicationUnloadedException : WorkflowApplicationException
	{
		public WorkflowApplicationUnloadedException () : this ("Workflow application has been unloaded") {}
		public WorkflowApplicationUnloadedException (string msg) : base (msg) {}
		public WorkflowApplicationUnloadedException (string msg, Exception inner) : base (msg, inner) {}
		protected WorkflowApplicationUnloadedException (SerializationInfo info, StreamingContext context) :
			base (info, context) {}
	}
}
