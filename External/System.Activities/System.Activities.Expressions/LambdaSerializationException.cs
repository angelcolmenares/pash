using System;
using System.Runtime.Serialization;

namespace System.Activities.Expressions
{
	[Serializable]
	public class LambdaSerializationException : Exception
	{
		public LambdaSerializationException () : this ("Lambda serialization error") {}
		public LambdaSerializationException (string msg) : base (msg) {}
		public LambdaSerializationException (string msg, Exception inner) : base (msg, inner) {}
		protected LambdaSerializationException (SerializationInfo info, StreamingContext context) :
			base (info, context) {}
	}
}
