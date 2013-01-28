using System;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;

namespace Microsoft.Management.Odata
{
	public sealed class SenderInfo
	{
		public Principal Principal
		{
			get;
			private set;
		}

		public Uri ResourceUri
		{
			get;
			private set;
		}

		internal SenderInfo(IIdentity identity, X509Certificate2 clientCertificate, Uri resourceUri)
		{
			this.Principal = new Principal(identity, clientCertificate);
			this.ResourceUri = resourceUri;
		}
	}
}