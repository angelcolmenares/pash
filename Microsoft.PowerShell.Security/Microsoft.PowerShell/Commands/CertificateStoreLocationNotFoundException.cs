using System;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.Commands
{
	[Serializable]
	public class CertificateStoreLocationNotFoundException : CertificateProviderItemNotFoundException
	{
		public CertificateStoreLocationNotFoundException()
		{
		}

		public CertificateStoreLocationNotFoundException(string message) : base(message)
		{
		}

		public CertificateStoreLocationNotFoundException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected CertificateStoreLocationNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		internal CertificateStoreLocationNotFoundException(Exception innerException) : base(innerException.Message, innerException)
		{
		}
	}
}