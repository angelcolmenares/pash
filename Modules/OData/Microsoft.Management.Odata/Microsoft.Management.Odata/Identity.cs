using System;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;

namespace Microsoft.Management.Odata
{
	public class Identity : IIdentity
	{

		public string AuthenticationType
		{
			get;set;
		}

		public X509Certificate2 Certificate
		{
			get;
			private set;
		}
		
		public bool IsAuthenticated
		{
			get;set;
		}
		
		public string Name
		{
			get;set;
		}

		internal Identity(IIdentity identity, X509Certificate2 clientCertificate)
		{
			this.AuthenticationType = identity.AuthenticationType;
			this.IsAuthenticated = identity.IsAuthenticated;
			this.Name = identity.Name;
			this.Certificate = clientCertificate;
		}
	}
}