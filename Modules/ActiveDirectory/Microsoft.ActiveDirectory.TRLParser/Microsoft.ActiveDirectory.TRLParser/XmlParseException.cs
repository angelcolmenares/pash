using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.ActiveDirectory.TRLParser
{
	[Serializable]
	public class XmlParseException : Exception
	{
		private const string TextErrorMessage = "ErrorMessage";

		private string _errMessage;

		public XmlParseException()
		{
		}

		public XmlParseException(string errorMessage) : base(errorMessage)
		{
			this._errMessage = errorMessage;
		}

		protected XmlParseException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public XmlParseException(string message, Exception innerException) : base(message, innerException)
		{
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			if (info != null)
			{
				info.AddValue("ErrorMessage", this._errMessage);
			}
		}
	}
}