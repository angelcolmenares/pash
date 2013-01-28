using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.PowerShell.Commands
{
	[Serializable]
	public class ServiceCommandException : SystemException
	{
		private string _serviceName;

		public string ServiceName
		{
			get
			{
				return this._serviceName;
			}
			set
			{
				this._serviceName = value;
			}
		}

		public ServiceCommandException()
		{
			this._serviceName = string.Empty;
			throw new NotImplementedException();
		}

		public ServiceCommandException(string message) : base(message)
		{
			this._serviceName = string.Empty;
		}

		public ServiceCommandException(string message, Exception innerException) : base(message, innerException)
		{
			this._serviceName = string.Empty;
		}

		protected ServiceCommandException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			this._serviceName = string.Empty;
			if (info != null)
			{
				this._serviceName = info.GetString("ServiceName");
				return;
			}
			else
			{
				throw new ArgumentNullException("info");
			}
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info != null)
			{
				base.GetObjectData(info, context);
				info.AddValue("ServiceName", this._serviceName);
				return;
			}
			else
			{
				throw new ArgumentNullException("info");
			}
		}
	}
}