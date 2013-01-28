using System;
using System.Runtime;
using System.Runtime.Serialization;

namespace System.DirectoryServices.Protocols
{
	[Serializable]
	public class DsmlInvalidDocumentException : DirectoryException
	{
		public DsmlInvalidDocumentException() : base(Res.GetString("InvalidDocument"))
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public DsmlInvalidDocumentException(string message) : base(message)
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public DsmlInvalidDocumentException(string message, Exception inner) : base(message, inner)
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		protected DsmlInvalidDocumentException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}