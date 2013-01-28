using System;
using System.Runtime;
using System.Runtime.Serialization;

namespace System.DirectoryServices.ActiveDirectory
{
	[Serializable]
	public class ActiveDirectoryObjectExistsException : Exception
	{
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ActiveDirectoryObjectExistsException(string message, Exception inner) : base(message, inner)
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ActiveDirectoryObjectExistsException(string message) : base(message)
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ActiveDirectoryObjectExistsException()
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		protected ActiveDirectoryObjectExistsException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}