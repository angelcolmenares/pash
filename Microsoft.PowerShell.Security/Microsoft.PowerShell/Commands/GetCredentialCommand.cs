using Microsoft.PowerShell.Security;
using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Get", "Credential", DefaultParameterSetName="CredentialSet", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113311")]
	[OutputType(new Type[] { typeof(PSCredential) }, ParameterSetName=new string[] { "CredentialSet", "MessageSet" })]
	public sealed class GetCredentialCommand : PSCmdlet
	{
		private const string credentialSet = "CredentialSet";

		private const string messageSet = "MessageSet";

		private PSCredential cred;

		private string message;

		private string userName;

		[Credential]
		[Parameter(Position=0, Mandatory=true, ParameterSetName="CredentialSet")]
		public PSCredential Credential
		{
			get
			{
				return this.cred;
			}
			set
			{
				this.cred = value;
			}
		}

		[Parameter(Mandatory=true, ParameterSetName="MessageSet")]
		public string Message
		{
			get
			{
				return this.message;
			}
			set
			{
				this.message = value;
			}
		}

		[Parameter(Position=0, Mandatory=false, ParameterSetName="MessageSet")]
		public string UserName
		{
			get
			{
				return this.userName;
			}
			set
			{
				this.userName = value;
			}
		}

		public GetCredentialCommand()
		{
		}

		protected override void BeginProcessing()
		{
			if (!string.IsNullOrEmpty(this.Message))
			{
				string promptForCredentialDefaultCaption = UtilsStrings.PromptForCredential_DefaultCaption;
				string message = this.Message;
				try
				{
					this.Credential = base.Host.UI.PromptForCredential(promptForCredentialDefaultCaption, message, this.userName, string.Empty);
				}
				catch (ArgumentException argumentException1)
				{
					ArgumentException argumentException = argumentException1;
					ErrorRecord errorRecord = new ErrorRecord(argumentException, "CouldNotPromptForCredential", ErrorCategory.InvalidOperation, null);
					base.WriteError(errorRecord);
				}
			}
			if (this.Credential != null)
			{
				base.WriteObject(this.Credential);
			}
		}
	}
}