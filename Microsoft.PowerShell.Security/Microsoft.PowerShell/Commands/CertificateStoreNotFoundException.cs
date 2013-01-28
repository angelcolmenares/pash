using System;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.Commands
{
	[Serializable]
	public class CertificateStoreNotFoundException : CertificateProviderItemNotFoundException
	{
		public CertificateStoreNotFoundException()
		{
		}

		public CertificateStoreNotFoundException(string message) : base(message)
		{
		}

		public CertificateStoreNotFoundException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected CertificateStoreNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		internal CertificateStoreNotFoundException(Exception innerException) : base(innerException.Message, innerException)
		{
		}
	}
}