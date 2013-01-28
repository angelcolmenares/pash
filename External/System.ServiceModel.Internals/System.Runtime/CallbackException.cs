using System;
using System.Runtime.Serialization;

namespace System.Runtime
{
	[Serializable]
	internal class CallbackException : FatalException
	{
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public CallbackException()
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public CallbackException(string message, Exception innerException) : base(message, innerException)
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		protected CallbackException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}