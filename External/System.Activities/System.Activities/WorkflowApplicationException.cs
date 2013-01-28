using System;
using System.Runtime.Serialization;

namespace System.Activities
{
	[Serializable]
	public class WorkflowApplicationException : Exception
	{
		public WorkflowApplicationException () : this ("Workfloa application error") {}
		public WorkflowApplicationException (string msg) : base (msg) {}
		public WorkflowApplicationException (string msg, Exception inner) : base (msg, inner) {}
		protected WorkflowApplicationException (SerializationInfo info, StreamingContext context) :
			base (info, context) {}
	}
}
