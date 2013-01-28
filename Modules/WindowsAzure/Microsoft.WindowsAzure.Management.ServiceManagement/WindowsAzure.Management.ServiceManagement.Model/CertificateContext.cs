using System;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Model
{
	public class CertificateContext : ServiceOperationContext
	{
		public string Data
		{
			get;
			set;
		}

		public string Thumbprint
		{
			get;
			set;
		}

		public string ThumbprintAlgorithm
		{
			get;
			set;
		}

		public Uri Url
		{
			get;
			set;
		}

		public CertificateContext()
		{
		}
	}
}