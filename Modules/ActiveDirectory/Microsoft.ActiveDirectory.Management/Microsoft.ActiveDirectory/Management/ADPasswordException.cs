using System;
using System.Runtime.Serialization;

namespace Microsoft.ActiveDirectory.Management
{
	[Serializable]
	public class ADPasswordException : ADInvalidOperationException
	{
		public ADPasswordException()
		{
		}

		public ADPasswordException(string message) : base(message)
		{
		}

		public ADPasswordException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected ADPasswordException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public ADPasswordException(string message, int errorCode) : base(message, errorCode)
		{
		}

		public ADPasswordException(string message, Exception innerException, int errorCode) : base(message, innerException, errorCode)
		{
		}

		public ADPasswordException(string message, Exception innerException, string serverErrorMessage) : base(message, innerException, serverErrorMessage)
		{
		}

		public ADPasswordException(string message, Exception innerException, int errorCode, string serverErrorMessage) : base(message, innerException, errorCode, serverErrorMessage)
		{
		}
	}
}