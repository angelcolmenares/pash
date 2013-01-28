using System;

namespace Microsoft.Management.PowerShellWebAccess.Primitives
{
	public class PromptForCredentialMessage : PromptMessageBase
	{
		public string Caption
		{
			get;
			private set;
		}

		public bool DomainCredentials
		{
			get;
			private set;
		}

		public string Message
		{
			get;
			private set;
		}

		public string UserName
		{
			get;
			private set;
		}

		internal PromptForCredentialMessage(string caption, string message, string userName, bool domainCredentials) : base((ClientMessageType)103)
		{
			this.Caption = caption;
			this.Message = message;
			this.UserName = userName;
			this.DomainCredentials = domainCredentials;
		}
	}
}