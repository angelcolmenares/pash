using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.ActiveDirectory.Management
{
	[Serializable]
	public class ADIdentityAlreadyExistsException : ADInvalidOperationException
	{
		public ADIdentityAlreadyExistsException()
		{
		}

		public ADIdentityAlreadyExistsException(string message) : base(message)
		{
		}

		public ADIdentityAlreadyExistsException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected ADIdentityAlreadyExistsException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public ADIdentityAlreadyExistsException(string message, int errorCode) : base(message, errorCode)
		{
		}

		public ADIdentityAlreadyExistsException(string message, Exception innerException, int errorCode) : base(message, innerException, errorCode)
		{
		}

		public ADIdentityAlreadyExistsException(string message, Exception innerException, int errorCode, string serverErrorMessage) : base(message, innerException, errorCode, serverErrorMessage)
		{
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info != null)
			{
				base.GetObjectData(info, context);
				return;
			}
			else
			{
				throw new ArgumentNullException("info");
			}
		}
	}
}