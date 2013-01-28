using System;
using System.Runtime;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;

namespace System.DirectoryServices.AccountManagement
{
	[Serializable]
	public class PrincipalServerDownException : PrincipalException
	{
		private int errorCode;

		private string serverName;

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public PrincipalServerDownException()
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public PrincipalServerDownException(string message) : base(message)
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public PrincipalServerDownException(string message, Exception innerException) : base(message, innerException)
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public PrincipalServerDownException(string message, int errorCode) : base(message)
		{
			this.errorCode = errorCode;
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public PrincipalServerDownException(string message, Exception innerException, int errorCode) : base(message, innerException)
		{
			this.errorCode = errorCode;
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public PrincipalServerDownException(string message, Exception innerException, int errorCode, string serverName) : base(message, innerException)
		{
			this.errorCode = errorCode;
			this.serverName = serverName;
		}

		protected PrincipalServerDownException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			this.errorCode = info.GetInt32("errorCode");
			this.serverName = (string)info.GetValue("serverName", typeof(string));
		}

		[SecurityCritical]
		[SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("errorCode", this.errorCode);
			info.AddValue("serverName", this.serverName, typeof(string));
			base.GetObjectData(info, context);
		}
	}
}