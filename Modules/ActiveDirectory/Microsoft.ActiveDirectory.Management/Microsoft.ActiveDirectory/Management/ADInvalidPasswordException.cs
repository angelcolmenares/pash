using System;
using System.Runtime.Serialization;

namespace Microsoft.ActiveDirectory.Management
{
	[Serializable]
	public class ADInvalidPasswordException : ADPasswordException
	{
		public ADInvalidPasswordException()
		{
		}

		public ADInvalidPasswordException(string message) : base(message)
		{
		}

		public ADInvalidPasswordException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected ADInvalidPasswordException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public ADInvalidPasswordException(string message, int errorCode) : base(message, errorCode)
		{
		}

		public ADInvalidPasswordException(string message, Exception innerException, int errorCode) : base(message, innerException, errorCode)
		{
		}

		public ADInvalidPasswordException(string message, Exception innerException, string serverErrorMessage) : base(message, innerException, serverErrorMessage)
		{
		}

		public ADInvalidPasswordException(string message, Exception innerException, int errorCode, string serverErrorMessage) : base(message, innerException, errorCode, serverErrorMessage)
		{
		}
	}
}