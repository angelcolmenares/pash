using System;
using System.Runtime;
using System.Runtime.Serialization;

namespace System.DirectoryServices.Protocols
{
	[Serializable]
	public class TlsOperationException : DirectoryOperationException
	{
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		protected TlsOperationException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public TlsOperationException()
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public TlsOperationException(string message) : base(message)
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public TlsOperationException(string message, Exception inner) : base(message, inner)
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public TlsOperationException(DirectoryResponse response) : base(response)
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public TlsOperationException(DirectoryResponse response, string message) : base(response, message)
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public TlsOperationException(DirectoryResponse response, string message, Exception inner) : base(response, message, inner)
		{
		}
	}
}