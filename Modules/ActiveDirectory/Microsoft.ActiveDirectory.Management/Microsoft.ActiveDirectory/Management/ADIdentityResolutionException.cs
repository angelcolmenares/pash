using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.ActiveDirectory.Management
{
	[Serializable]
	public class ADIdentityResolutionException : Exception
	{
		public ADIdentityResolutionException()
		{
		}

		public ADIdentityResolutionException(string message) : base(message)
		{
		}

		public ADIdentityResolutionException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected ADIdentityResolutionException(SerializationInfo info, StreamingContext context) : base(info, context)
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