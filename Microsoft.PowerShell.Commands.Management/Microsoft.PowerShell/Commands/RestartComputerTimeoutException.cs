using System;
using System.Management.Automation;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.PowerShell.Commands
{
	[Serializable]
	public sealed class RestartComputerTimeoutException : RuntimeException
	{
		public string ComputerName
		{
			get;
			private set;
		}

		public int Timeout
		{
			get;
			private set;
		}

		internal RestartComputerTimeoutException(string computerName, int timeout, string message, string errorId) : base(message)
		{
			base.SetErrorId(errorId);
			base.SetErrorCategory(ErrorCategory.OperationTimeout);
			this.ComputerName = computerName;
			this.Timeout = timeout;
		}

		public RestartComputerTimeoutException()
		{
		}

		public RestartComputerTimeoutException(string message) : base(message)
		{
		}

		public RestartComputerTimeoutException(string message, Exception innerException) : base(message, innerException)
		{
		}

		private RestartComputerTimeoutException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			if (info != null)
			{
				this.ComputerName = info.GetString("ComputerName");
				this.Timeout = info.GetInt32("Timeout");
				return;
			}
			else
			{
				throw new PSArgumentNullException("info");
			}
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info != null)
			{
				base.GetObjectData(info, context);
				info.AddValue("ComputerName", this.ComputerName);
				info.AddValue("Timeout", this.Timeout);
				return;
			}
			else
			{
				throw new PSArgumentNullException("info");
			}
		}
	}
}