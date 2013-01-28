using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.ActiveDirectory.Management
{
	[Serializable]
	public class ADException : Exception, IHasErrorCode, IHasServerErrorMessage
	{
		private int _errorCode;

		private string _serverErrorMessage;

		public int ErrorCode
		{
			get
			{
				return this._errorCode;
			}
		}

		public string ServerErrorMessage
		{
			get
			{
				return this._serverErrorMessage;
			}
		}

		public ADException()
		{
		}

		public ADException(string message) : base(message)
		{
		}

		public ADException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected ADException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			this._errorCode = info.GetInt32("errorCode");
			this._serverErrorMessage = (string)info.GetValue("serverErrorMessage", typeof(string));
		}

		public ADException(string message, int errorCode) : base(message)
		{
			this._errorCode = errorCode;
		}

		public ADException(string message, Exception inner, int errorCode) : base(message, inner)
		{
			this._errorCode = errorCode;
		}

		public ADException(string message, string serverErrorMessage) : base(message)
		{
			this._serverErrorMessage = serverErrorMessage;
		}

		public ADException(string message, int errorCode, string serverErrorMessage) : base(message)
		{
			this._errorCode = errorCode;
			this._serverErrorMessage = serverErrorMessage;
		}

		public ADException(string message, Exception inner, string serverErrorMessage) : base(message, inner)
		{
			this._serverErrorMessage = serverErrorMessage;
		}

		public ADException(string message, Exception inner, int errorCode, string serverErrorMessage) : base(message, inner)
		{
			this._errorCode = errorCode;
			this._serverErrorMessage = serverErrorMessage;
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext streamingContext)
		{
			if (info != null)
			{
				info.AddValue("errorCode", this._errorCode);
				info.AddValue("serverErrorMessage", this._serverErrorMessage, typeof(string));
				base.GetObjectData(info, streamingContext);
				return;
			}
			else
			{
				throw new ArgumentNullException("info");
			}
		}
	}
}