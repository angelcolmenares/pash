using System;
using System.Runtime.Serialization;

namespace System.Activities
{
	[Serializable]
	public class InvalidWorkflowException : Exception
	{
		public InvalidWorkflowException () : this ("Invalid workflow") {}
		public InvalidWorkflowException (string msg) : base (msg) {}
		public InvalidWorkflowException (string msg, Exception inner) : base (msg, inner) {}
		protected InvalidWorkflowException (SerializationInfo info, StreamingContext context) :
			base (info, context) {}
	}
}
