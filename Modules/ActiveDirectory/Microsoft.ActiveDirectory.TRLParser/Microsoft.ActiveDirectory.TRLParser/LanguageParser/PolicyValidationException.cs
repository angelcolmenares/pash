using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.ActiveDirectory.TRLParser.LanguageParser
{
	[Serializable]
	public class PolicyValidationException : Exception
	{
		public PolicyValidationException()
		{
		}

		public PolicyValidationException(string message) : base(message)
		{
		}

		public PolicyValidationException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected PolicyValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
		}
	}
}