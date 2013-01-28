using System;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.Commands
{
	[Serializable]
	public class CertificateProviderItemNotFoundException : SystemException
	{
		public CertificateProviderItemNotFoundException()
		{
		}

		public CertificateProviderItemNotFoundException(string message) : base(message)
		{
		}

		public CertificateProviderItemNotFoundException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected CertificateProviderItemNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		internal CertificateProviderItemNotFoundException(Exception innerException) : base(innerException.Message, innerException)
		{
		}
	}
}