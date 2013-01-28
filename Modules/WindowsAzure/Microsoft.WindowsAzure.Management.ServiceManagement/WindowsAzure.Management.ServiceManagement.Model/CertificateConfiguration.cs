using System;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Model
{
	public class CertificateConfiguration
	{
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

		public CertificateConfiguration()
		{
		}
	}
}