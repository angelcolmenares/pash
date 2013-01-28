using System;
using System.Management.Automation;

namespace Microsoft.WSMan.Management
{
	public class AuthenticatingWSManCommand : PSCmdlet
	{
		private PSCredential credential;

		private AuthenticationMechanism authentication;

		private string thumbPrint;

		[Alias(new string[] { "auth", "am" })]
		[Parameter]
		[ValidateNotNullOrEmpty]
		public virtual AuthenticationMechanism Authentication
		{
			get
			{
				return this.authentication;
			}
			set
			{
				this.authentication = value;
				this.ValidateSpecifiedAuthentication();
			}
		}

		[Parameter]
		[ValidateNotNullOrEmpty]
		public virtual string CertificateThumbprint
		{
			get
			{
				return this.thumbPrint;
			}
			set
			{
				this.thumbPrint = value;
				this.ValidateSpecifiedAuthentication();
			}
		}

		[Alias(new string[] { "cred", "c" })]
		[Credential]
		[Parameter(ValueFromPipelineByPropertyName=true)]
		[ValidateNotNullOrEmpty]
		public virtual PSCredential Credential
		{
			get
			{
				return this.credential;
			}
			set
			{
				this.credential = value;
				this.ValidateSpecifiedAuthentication();
			}
		}

		public AuthenticatingWSManCommand()
		{
			this.authentication = AuthenticationMechanism.Default;
		}

		internal void ValidateSpecifiedAuthentication()
		{
			WSManHelper.ValidateSpecifiedAuthentication(this.Authentication, this.Credential, this.CertificateThumbprint);
		}
	}
}