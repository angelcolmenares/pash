using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.PowerShell.Commands
{
	[Serializable]
	public class ProcessCommandException : SystemException
	{
		private string _processName;

		public string ProcessName
		{
			get
			{
				return this._processName;
			}
			set
			{
				this._processName = value;
			}
		}

		public ProcessCommandException()
		{
			this._processName = string.Empty;
			throw new NotImplementedException();
		}

		public ProcessCommandException(string message) : base(message)
		{
			this._processName = string.Empty;
		}

		public ProcessCommandException(string message, Exception innerException) : base(message, innerException)
		{
			this._processName = string.Empty;
		}

		protected ProcessCommandException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			this._processName = string.Empty;
			this._processName = info.GetString("ProcessName");
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			if (info != null)
			{
				info.AddValue("ProcessName", this._processName);
				return;
			}
			else
			{
				throw new ArgumentNullException("info");
			}
		}
	}
}