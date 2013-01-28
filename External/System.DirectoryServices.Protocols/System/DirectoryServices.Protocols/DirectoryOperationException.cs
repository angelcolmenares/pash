using System;
using System.Runtime;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.DirectoryServices.Protocols
{
	[Serializable]
	public class DirectoryOperationException : DirectoryException, ISerializable
	{
		internal DirectoryResponse response;

		public DirectoryResponse Response
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.response;
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		protected DirectoryOperationException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public DirectoryOperationException()
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public DirectoryOperationException(string message) : base(message)
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public DirectoryOperationException(string message, Exception inner) : base(message, inner)
		{
		}

		public DirectoryOperationException(DirectoryResponse response) : base(Res.GetString("DefaultOperationsError"))
		{
			this.response = response;
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public DirectoryOperationException(DirectoryResponse response, string message) : base(message)
		{
			this.response = response;
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public DirectoryOperationException(DirectoryResponse response, string message, Exception inner) : base(message, inner)
		{
			this.response = response;
		}

		[SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter=true)]
		public override void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
			base.GetObjectData(serializationInfo, streamingContext);
		}
	}
}