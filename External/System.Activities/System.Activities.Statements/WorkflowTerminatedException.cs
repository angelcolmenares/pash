using System;
using System.Runtime.Serialization;

namespace System.Activities.Statements
{
	[Serializable]
	public class WorkflowTerminatedException : Exception
	{
		public WorkflowTerminatedException () : this ("Lambda serialization error") {}
		public WorkflowTerminatedException (string msg) : base (msg) {}
		public WorkflowTerminatedException (string msg, Exception inner) : base (msg, inner) {}
		protected WorkflowTerminatedException (SerializationInfo info, StreamingContext context) :
			base (info, context) {}
	}
}
