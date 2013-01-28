using System;
using System.Runtime.Serialization;

namespace System.Runtime
{
	[Serializable]
	internal class FatalException : SystemException
	{
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public FatalException()
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public FatalException(string message) : base(message)
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public FatalException(string message, Exception innerException) : base(message, innerException)
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		protected FatalException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}