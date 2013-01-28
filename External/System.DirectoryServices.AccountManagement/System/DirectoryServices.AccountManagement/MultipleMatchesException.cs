using System;
using System.Runtime;
using System.Runtime.Serialization;

namespace System.DirectoryServices.AccountManagement
{
	[Serializable]
	public class MultipleMatchesException : PrincipalException
	{
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public MultipleMatchesException()
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public MultipleMatchesException(string message) : base(message)
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public MultipleMatchesException(string message, Exception innerException) : base(message, innerException)
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		protected MultipleMatchesException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}