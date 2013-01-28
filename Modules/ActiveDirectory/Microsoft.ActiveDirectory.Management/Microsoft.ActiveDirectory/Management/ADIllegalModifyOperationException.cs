using System;
using System.Runtime.Serialization;

namespace Microsoft.ActiveDirectory.Management
{
	[Serializable]
	public class ADIllegalModifyOperationException : ADInvalidOperationException
	{
		public ADIllegalModifyOperationException()
		{
		}

		public ADIllegalModifyOperationException(string message) : base(message)
		{
		}

		public ADIllegalModifyOperationException(string message, Exception innerException) : base(message, innerException)
		{
		}

		public ADIllegalModifyOperationException(string message, int errorCode) : base(message, errorCode)
		{
		}

		public ADIllegalModifyOperationException(string message, Exception innerException, int errorCode) : base(message, innerException, errorCode)
		{
		}

		public ADIllegalModifyOperationException(string message, Exception innerException, int errorCode, string serverErrorMessage) : base(message, innerException, errorCode, serverErrorMessage)
		{
		}

		protected ADIllegalModifyOperationException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}