using System;
using System.Runtime;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;

namespace System.DirectoryServices.AccountManagement
{
	[Serializable]
	public class PrincipalOperationException : PrincipalException
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
		public PrincipalOperationException()
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public PrincipalOperationException(string message) : base(message)
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public PrincipalOperationException(string message, Exception innerException) : base(message, innerException)
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public PrincipalOperationException(string message, int errorCode) : base(message)
		{
			this.errorCode = errorCode;
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public PrincipalOperationException(string message, Exception innerException, int errorCode) : base(message, innerException)
		{
			this.errorCode = errorCode;
		}

		protected PrincipalOperationException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			this.errorCode = info.GetInt32("errorCode");
		}

		[SecurityCritical]
		[SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("errorCode", this.errorCode);
			base.GetObjectData(info, context);
		}
	}
}