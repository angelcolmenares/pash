using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.ActiveDirectory.Management
{
	[Serializable]
	public class ADServerDownException : Exception, IHasErrorCode
	{
		private int _errorCode;

		private string _serverName;

		public int ErrorCode
		{
			get
			{
				return this._errorCode;
			}
		}

		public string ServerName
		{
			get
			{
				return this._serverName;
			}
		}

		public ADServerDownException()
		{
		}

		public ADServerDownException(string message) : base(message)
		{
		}

		public ADServerDownException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected ADServerDownException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			this._errorCode = info.GetInt32("errorCode");
			this._serverName = (string)info.GetValue("serverName", typeof(string));
		}

		public ADServerDownException(string message, int errorCode) : base(message)
		{
			this._errorCode = errorCode;
		}

		public ADServerDownException(string message, string serverName) : base(message)
		{
			this._serverName = serverName;
		}

		public ADServerDownException(string message, string serverName, int errorCode) : base(message)
		{
			this._errorCode = errorCode;
			this._serverName = serverName;
		}

		public ADServerDownException(string message, Exception innerException, string serverName) : base(message, innerException)
		{
			this._serverName = serverName;
		}

		public ADServerDownException(string message, Exception innerException, string serverName, int errorCode) : base(message, innerException)
		{
			this._errorCode = errorCode;
			this._serverName = serverName;
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info != null)
			{
				info.AddValue("errorCode", this._errorCode);
				info.AddValue("serverName", this._serverName, typeof(string));
				base.GetObjectData(info, context);
				return;
			}
			else
			{
				throw new ArgumentNullException("info");
			}
		}
	}
}