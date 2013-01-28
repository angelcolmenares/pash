using System;
using System.Runtime;
using System.Runtime.Serialization;

namespace System.DirectoryServices.AccountManagement
{
	[Serializable]
	public class PrincipalExistsException : PrincipalException
	{
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public PrincipalExistsException()
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public PrincipalExistsException(string message) : base(message)
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public PrincipalExistsException(string message, Exception innerException) : base(message, innerException)
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		protected PrincipalExistsException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}