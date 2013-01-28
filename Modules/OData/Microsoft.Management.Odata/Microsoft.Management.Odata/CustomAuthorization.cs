using System;
using System.Security.Principal;

namespace Microsoft.Management.Odata
{
	public abstract class CustomAuthorization : IDisposable
	{
		protected CustomAuthorization()
		{
		}

		public abstract WindowsIdentity AuthorizeUser(SenderInfo senderInfo, out UserQuota userQuota);

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool isDisposing)
		{
		}

		public abstract string GetMembershipId(SenderInfo senderInfo);
	}
}