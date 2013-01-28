using System;
using System.Runtime;
using System.Runtime.Serialization;

namespace System.DirectoryServices.AccountManagement
{
	[Serializable]
	public class PasswordException : PrincipalException
	{
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public PasswordException()
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public PasswordException(string message) : base(message)
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public PasswordException(string message, Exception innerException) : base(message, innerException)
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		protected PasswordException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}