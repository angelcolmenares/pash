using System;
using System.Runtime.Serialization;

namespace System.Activities
{
	[Serializable]
	public class ValidationException : SystemException
	{
		public ValidationException () : this ("validation error") {}
		public ValidationException (string msg) : base (msg) {}
		public ValidationException (string msg, Exception inner) : base (msg, inner) {}
		protected ValidationException (SerializationInfo info, StreamingContext context) :
			base (info, context) {}
	}
}
