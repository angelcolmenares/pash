using System;
using System.Runtime;
using System.Runtime.Serialization;

namespace System.DirectoryServices.AccountManagement
{
	[Serializable]
	public class NoMatchingPrincipalException : PrincipalException
	{
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public NoMatchingPrincipalException()
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public NoMatchingPrincipalException(string message) : base(message)
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public NoMatchingPrincipalException(string message, Exception innerException) : base(message, innerException)
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		protected NoMatchingPrincipalException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}