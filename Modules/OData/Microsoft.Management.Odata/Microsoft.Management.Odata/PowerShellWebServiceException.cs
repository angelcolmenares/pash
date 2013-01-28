using Microsoft.Management.Odata.Common;
using System;
using System.Runtime.Serialization;
using System.Text;

namespace Microsoft.Management.Odata
{
	[Serializable]
	public class PowerShellWebServiceException : Exception
	{
		public PowerShellWebServiceException()
		{
		}

		public PowerShellWebServiceException(string message) : base(message)
		{
		}

		public PowerShellWebServiceException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected PowerShellWebServiceException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		protected virtual StringBuilder ToTraceMessage(StringBuilder builder)
		{
			builder.AppendLine(string.Concat("Type = ", base.GetType()));
			if (!string.IsNullOrEmpty(this.Message))
			{
				builder.AppendLine(string.Concat("Message = ", this.Message));
			}
			if (base.InnerException != null)
			{
				builder.AppendLine(string.Concat("Inner Exception = ", base.InnerException.ToTraceMessage("Exception")));
			}
			return builder;
		}

		protected void Trace()
		{
			if (TraceHelper.IsEnabled(5))
			{
				TraceHelper.Current.DebugMessage(string.Concat("PowerShellWebServiceException occurred\n", this.ToTraceMessage(new StringBuilder())));
			}
		}
	}
}