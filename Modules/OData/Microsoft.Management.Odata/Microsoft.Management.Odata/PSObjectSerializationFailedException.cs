using Microsoft.Management.Odata.Common;
using System;
using System.Runtime.Serialization;

namespace Microsoft.Management.Odata
{
	[Serializable]
	public class PSObjectSerializationFailedException : PowerShellWebServiceException
	{
		public PSObjectSerializationFailedException()
		{
			base.Trace();
		}

		public PSObjectSerializationFailedException(string message) : base(message)
		{
			TraceHelper.Current.PSObjectSerializationFailedException(message);
			base.Trace();
		}

		public PSObjectSerializationFailedException(string message, Exception innerException) : base(message, innerException)
		{
			base.Trace();
		}

		protected PSObjectSerializationFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}