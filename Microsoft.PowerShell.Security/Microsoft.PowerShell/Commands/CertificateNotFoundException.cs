using System;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.Commands
{
	[Serializable]
	public class CertificateNotFoundException : CertificateProviderItemNotFoundException
	{
		public CertificateNotFoundException()
		{
		}

		public CertificateNotFoundException(string message) : base(message)
		{
		}

		public CertificateNotFoundException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected CertificateNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		internal CertificateNotFoundException(Exception innerException) : base(innerException.Message, innerException)
		{
		}
	}
}