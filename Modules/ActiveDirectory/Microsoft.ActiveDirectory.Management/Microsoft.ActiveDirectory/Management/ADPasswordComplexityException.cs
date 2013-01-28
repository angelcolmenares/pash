using System;
using System.Runtime.Serialization;

namespace Microsoft.ActiveDirectory.Management
{
	[Serializable]
	public class ADPasswordComplexityException : ADPasswordException
	{
		public ADPasswordComplexityException()
		{
		}

		public ADPasswordComplexityException(string message) : base(message)
		{
		}

		public ADPasswordComplexityException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected ADPasswordComplexityException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public ADPasswordComplexityException(string message, int errorCode) : base(message, errorCode)
		{
		}

		public ADPasswordComplexityException(string message, Exception innerException, int errorCode) : base(message, innerException, errorCode)
		{
		}

		public ADPasswordComplexityException(string message, Exception innerException, string serverErrorMessage) : base(message, innerException, serverErrorMessage)
		{
		}

		public ADPasswordComplexityException(string message, Exception innerException, int errorCode, string serverErrorMessage) : base(message, innerException, errorCode, serverErrorMessage)
		{
		}
	}
}