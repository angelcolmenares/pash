using System;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;

namespace Microsoft.Management.Odata
{
	public class Principal : IPrincipal
	{
		public Identity Identity
		{
			get;
			private set;
		}

		IIdentity System.Security.Principal.IPrincipal.Identity
		{
			get
			{
				return this.Identity;
			}
		}

		public WindowsIdentity WindowsIdentity
		{
			get;
			private set;
		}

		internal Principal(IIdentity identity, X509Certificate2 clientCertificate)
		{
			this.Identity = new Identity(identity, clientCertificate);
			this.WindowsIdentity = identity as WindowsIdentity;
		}

		public bool IsInRole(string role)
		{
			if (this.WindowsIdentity == null)
			{
				return false;
			}
			else
			{
				WindowsPrincipal windowsPrincipal = new WindowsPrincipal(this.WindowsIdentity);
				return windowsPrincipal.IsInRole(role);
			}
		}
	}
}