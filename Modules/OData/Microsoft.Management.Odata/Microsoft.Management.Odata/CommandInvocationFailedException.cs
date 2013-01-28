using Microsoft.Management.Odata.Common;
using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace Microsoft.Management.Odata
{
	[Serializable]
	public class CommandInvocationFailedException : PowerShellWebServiceException
	{
		public string CommandName
		{
			get;
			private set;
		}

		public CommandInvocationFailedException()
		{
			base.Trace();
		}

		public CommandInvocationFailedException(string command, Exception innerException) : base(CommandInvocationFailedException.GetExceptionMessage(command, innerException), innerException)
		{
			this.CommandName = command;
			if (innerException == null)
			{
				TraceHelper.Current.CommandInvocationFailedException(command, string.Empty, string.Empty);
			}
			else
			{
				TraceHelper.Current.CommandInvocationFailedException(command, innerException.GetType().ToString(), innerException.Message);
			}
			base.Trace();
		}

		public CommandInvocationFailedException(string message) : base(message)
		{
			base.Trace();
		}

		protected CommandInvocationFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			if (info != null)
			{
				this.CommandName = info.GetString("CommandName");
			}
		}

		private static string GetExceptionMessage(string command, Exception innerException)
		{
			if (innerException != null)
			{
				object[] message = new object[2];
				message[0] = command;
				message[1] = innerException.Message;
				return string.Format(CultureInfo.CurrentCulture, Resources.CommandExecutionFailed, message);
			}
			else
			{
				object[] objArray = new object[1];
				objArray[0] = command;
				return string.Format(CultureInfo.CurrentCulture, Resources.CommandExecutionFailedWithoutCause, objArray);
			}
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			if (info != null)
			{
				info.AddValue("CommandName", this.CommandName);
			}
		}
	}
}