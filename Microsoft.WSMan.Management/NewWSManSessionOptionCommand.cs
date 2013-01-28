using System;
using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;
using System.Net;

namespace Microsoft.WSMan.Management
{
	[Cmdlet("New", "WSManSessionOption", HelpUri="http://go.microsoft.com/fwlink/?LinkId=141449")]
	public class NewWSManSessionOptionCommand : PSCmdlet
	{
		private ProxyAccessType _proxyacesstype;

		private ProxyAuthentication proxyauthentication;

		private PSCredential _proxycredential;

		private bool skipcacheck;

		private bool skipcncheck;

		private bool skiprevocationcheck;

		private int spnport;

		private int operationtimeout;

		private bool noencryption;

		private bool useutf16;

		[Parameter]
		public SwitchParameter NoEncryption
		{
			get
			{
				return this.noencryption;
			}
			set
			{
				this.noencryption = value;
			}
		}

		[Alias(new string[] { "OperationTimeoutMSec" })]
		[Parameter]
		[ValidateRange(0, 0x7fffffff)]
		public int OperationTimeout
		{
			get
			{
				return this.operationtimeout;
			}
			set
			{
				this.operationtimeout = value;
			}
		}

		[Parameter]
		[ValidateNotNullOrEmpty]
		public ProxyAccessType ProxyAccessType
		{
			get
			{
				return this._proxyacesstype;
			}
			set
			{
				this._proxyacesstype = value;
			}
		}

		[Parameter]
		[ValidateNotNullOrEmpty]
		public ProxyAuthentication ProxyAuthentication
		{
			get
			{
				return this.proxyauthentication;
			}
			set
			{
				this.proxyauthentication = value;
			}
		}

		[Credential]
		[Parameter]
		[ValidateNotNullOrEmpty]
		public PSCredential ProxyCredential
		{
			get
			{
				return this._proxycredential;
			}
			set
			{
				this._proxycredential = value;
			}
		}

		[Parameter]
		public SwitchParameter SkipCACheck
		{
			get
			{
				return this.skipcacheck;
			}
			set
			{
				this.skipcacheck = value;
			}
		}

		[Parameter]
		public SwitchParameter SkipCNCheck
		{
			get
			{
				return this.skipcncheck;
			}
			set
			{
				this.skipcncheck = value;
			}
		}

		[Parameter]
		public SwitchParameter SkipRevocationCheck
		{
			get
			{
				return this.skiprevocationcheck;
			}
			set
			{
				this.skiprevocationcheck = value;
			}
		}

		[Parameter]
		[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId="SPN")]
		[ValidateRange(0, 0x7fffffff)]
		public int SPNPort
		{
			get
			{
				return this.spnport;
			}
			set
			{
				this.spnport = value;
			}
		}

		[Parameter]
		[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId="UTF")]
		public SwitchParameter UseUTF16
		{
			get
			{
				return this.useutf16;
			}
			set
			{
				this.useutf16 = value;
			}
		}

		public NewWSManSessionOptionCommand()
		{
		}

		protected override void BeginProcessing()
		{
			WSManHelper wSManHelper = new WSManHelper(this);
			if ((this.proxyauthentication.Equals(ProxyAuthentication.Basic) || this.proxyauthentication.Equals(ProxyAuthentication.Digest)) && this._proxycredential == null)
			{
				InvalidOperationException invalidOperationException = new InvalidOperationException(wSManHelper.GetResourceMsgFromResourcetext("NewWSManSessionOptionCred"));
				ErrorRecord errorRecord = new ErrorRecord(invalidOperationException, "InvalidOperationException", ErrorCategory.InvalidOperation, null);
				base.WriteError(errorRecord);
				return;
			}
			else
			{
				if (this._proxycredential == null || this.proxyauthentication != 0)
				{
					SessionOption sessionOption = new SessionOption();
					sessionOption.SPNPort = this.spnport;
					sessionOption.UseUtf16 = this.useutf16;
					sessionOption.SkipCNCheck = this.skipcncheck;
					sessionOption.SkipCACheck = this.skipcacheck;
					sessionOption.OperationTimeout = this.operationtimeout;
					sessionOption.SkipRevocationCheck = this.skiprevocationcheck;
					sessionOption.ProxyAccessType = this._proxyacesstype;
					sessionOption.ProxyAuthentication = this.proxyauthentication;
					if (this.noencryption)
					{
						sessionOption.UseEncryption = false;
					}
					if (this._proxycredential != null)
					{
						NetworkCredential networkCredential = this._proxycredential.GetNetworkCredential();
						sessionOption.ProxyCredential = networkCredential;
					}
					base.WriteObject(sessionOption);
					return;
				}
				else
				{
					InvalidOperationException invalidOperationException1 = new InvalidOperationException(wSManHelper.GetResourceMsgFromResourcetext("NewWSManSessionOptionAuth"));
					ErrorRecord errorRecord1 = new ErrorRecord(invalidOperationException1, "InvalidOperationException", ErrorCategory.InvalidOperation, null);
					base.WriteError(errorRecord1);
					return;
				}
			}
		}
	}
}