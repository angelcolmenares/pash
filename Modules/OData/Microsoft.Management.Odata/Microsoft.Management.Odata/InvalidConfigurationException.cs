using Microsoft.Management.Odata.Common;
using System;
using System.Runtime.Serialization;

namespace Microsoft.Management.Odata
{
	[Serializable]
	public class InvalidConfigurationException : PowerShellWebServiceException
	{
		public InvalidConfigurationException()
		{
			base.Trace();
		}

		public InvalidConfigurationException(string message) : base(message, null)
		{
			TraceHelper.Current.InvalidDataServiceConfiguration(message);
			base.Trace();
		}

		public InvalidConfigurationException(string message, Exception innerException) : base(message, innerException)
		{
			TraceHelper.Current.InvalidDataServiceConfiguration(message);
			base.Trace();
		}

		protected InvalidConfigurationException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}