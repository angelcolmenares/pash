using System;
using System.Runtime;
using System.Runtime.Serialization;

namespace System.DirectoryServices.AccountManagement
{
	[Serializable]
	public abstract class PrincipalException : SystemException
	{
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		internal PrincipalException()
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		internal PrincipalException(string message) : base(message)
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		internal PrincipalException(string message, Exception innerException) : base(message, innerException)
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		internal PrincipalException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}