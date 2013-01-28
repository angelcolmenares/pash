using System;
using System.DirectoryServices;
using System.Runtime;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.DirectoryServices.ActiveDirectory
{
	[Serializable]
	public class ActiveDirectoryOperationException : Exception, ISerializable
	{
		private int errorCode;

		public int ErrorCode
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.errorCode;
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ActiveDirectoryOperationException(string message, Exception inner, int errorCode) : base(message, inner)
		{
			this.errorCode = errorCode;
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ActiveDirectoryOperationException(string message, int errorCode) : base(message)
		{
			this.errorCode = errorCode;
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ActiveDirectoryOperationException(string message, Exception inner) : base(message, inner)
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ActiveDirectoryOperationException(string message) : base(message)
		{
		}

		public ActiveDirectoryOperationException() : base(Res.GetString("DSUnknownFailure"))
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		protected ActiveDirectoryOperationException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		[SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter=true)]
		public override void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
			base.GetObjectData(serializationInfo, streamingContext);
		}
	}
}