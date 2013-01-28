using System;
using System.Runtime;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.DirectoryServices.Protocols
{
	[Serializable]
	public class ErrorResponseException : DirectoryException, ISerializable
	{
		private DsmlErrorResponse errorResponse;

		public DsmlErrorResponse Response
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.errorResponse;
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		protected ErrorResponseException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ErrorResponseException()
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ErrorResponseException(string message) : base(message)
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ErrorResponseException(string message, Exception inner) : base(message, inner)
		{
		}

		public ErrorResponseException(DsmlErrorResponse response) : this(response, Res.GetString("ErrorResponse"), null)
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ErrorResponseException(DsmlErrorResponse response, string message) : this(response, message, null)
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ErrorResponseException(DsmlErrorResponse response, string message, Exception inner) : base(message, inner)
		{
			this.errorResponse = response;
		}

		[SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter=true)]
		public override void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
			base.GetObjectData(serializationInfo, streamingContext);
		}
	}
}