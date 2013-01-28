using Microsoft.Management.Odata.Common;
using System;
using System.Runtime.Serialization;
using System.Text;

namespace Microsoft.Management.Odata
{
	[Serializable]
	public class UnauthorizedAccessException : PowerShellWebServiceException
	{
		public string AuthenticationType
		{
			get;
			private set;
		}

		public bool IsAuthenticated
		{
			get;
			private set;
		}

		public string UserName
		{
			get;
			private set;
		}

		public UnauthorizedAccessException()
		{
			base.Trace();
		}

		public UnauthorizedAccessException(string userName, string authenticationType, bool isAuthenticated) : base(userName)
		{
			TraceHelper.Current.UnauthorizedAccessException(userName, authenticationType);
			this.UserName = userName;
			this.AuthenticationType = authenticationType;
			this.IsAuthenticated = isAuthenticated;
			base.Trace();
		}

		public UnauthorizedAccessException(string message) : base(message)
		{
			base.Trace();
		}

		public UnauthorizedAccessException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected UnauthorizedAccessException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			this.UserName = info.GetString("UserName");
			this.AuthenticationType = info.GetString("AuthenticationType");
			this.IsAuthenticated = bool.Parse(info.GetString("IsAuthenticated"));
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			if (info != null)
			{
				info.AddValue("UserName", this.UserName);
				info.AddValue("AuthenticationType", this.AuthenticationType);
				bool isAuthenticated = this.IsAuthenticated;
				info.AddValue("IsAuthenticated", isAuthenticated.ToString());
			}
		}

		protected override StringBuilder ToTraceMessage(StringBuilder builder)
		{
			builder = base.ToTraceMessage(builder);
			builder.AppendLine(string.Concat("UserName = ", this.UserName));
			builder.AppendLine(string.Concat("Authentication Type = ", this.AuthenticationType));
			builder.AppendLine(string.Concat("Is Authenticated = ", this.IsAuthenticated));
			return builder;
		}
	}
}