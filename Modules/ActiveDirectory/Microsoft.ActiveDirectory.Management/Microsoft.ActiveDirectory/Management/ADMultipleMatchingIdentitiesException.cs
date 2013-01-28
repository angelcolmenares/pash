using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.ActiveDirectory.Management
{
	[Serializable]
	public class ADMultipleMatchingIdentitiesException : ADIdentityResolutionException
	{
		public ADMultipleMatchingIdentitiesException()
		{
		}

		public ADMultipleMatchingIdentitiesException(string message) : base(message)
		{
		}

		public ADMultipleMatchingIdentitiesException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected ADMultipleMatchingIdentitiesException(SerializationInfo info, StreamingContext context) : base(info, context)
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