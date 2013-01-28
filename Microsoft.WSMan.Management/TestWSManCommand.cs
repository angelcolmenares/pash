using System;
using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;
using System.Xml;

namespace Microsoft.WSMan.Management
{
	[Cmdlet("Test", "WSMan", HelpUri="http://go.microsoft.com/fwlink/?LinkId=141464")]
	public class TestWSManCommand : AuthenticatingWSManCommand, IDisposable
	{
		private string computername;

		private AuthenticationMechanism authentication;

		private int port;

		private SwitchParameter usessl;

		private string applicationname;

		[Parameter(ParameterSetName="ComputerName")]
		[ValidateNotNullOrEmpty]
		public string ApplicationName
		{
			get
			{
				return this.applicationname;
			}
			set
			{
				this.applicationname = value;
			}
		}

		[Alias(new string[] { "auth", "am" })]
		[Parameter]
		[ValidateNotNullOrEmpty]
		public override AuthenticationMechanism Authentication
		{
			get
			{
				return this.authentication;
			}
			set
			{
				this.authentication = value;
				base.ValidateSpecifiedAuthentication();
			}
		}

		[Alias(new string[] { "cn" })]
		[Parameter(Position=0, ValueFromPipeline=true)]
		public string ComputerName
		{
			get
			{
				return this.computername;
			}
			set
			{
				this.computername = value;
				if (string.IsNullOrEmpty(this.computername) || this.computername.Equals(".", StringComparison.CurrentCultureIgnoreCase))
				{
					this.computername = "localhost";
				}
			}
		}

		[Parameter(ParameterSetName="ComputerName")]
		[ValidateNotNullOrEmpty]
		[ValidateRange(1, 0x7fffffff)]
		public int Port
		{
			get
			{
				return this.port;
			}
			set
			{
				this.port = value;
			}
		}

		[Parameter(ParameterSetName="ComputerName")]
		[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId="SSL")]
		public SwitchParameter UseSSL
		{
			get
			{
				return this.usessl;
			}
			set
			{
				this.usessl = value;
			}
		}

		public TestWSManCommand()
		{
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
		}

		public void Dispose(IWSManSession sessionObject)
		{
			sessionObject = null;
			this.Dispose();
		}

		protected override void ProcessRecord()
		{
			WSManHelper wSManHelper = new WSManHelper(this);
			IWSManEx wSManClass = (IWSManEx)(new WSManClass());
			string str = wSManHelper.CreateConnectionString(null, this.port, this.computername, this.applicationname);
			IWSManSession wSManSession = null;
			try
			{
				try
				{
					wSManSession = wSManHelper.CreateSessionObject(wSManClass, this.Authentication, null, this.Credential, str, this.CertificateThumbprint, this.usessl.IsPresent);
					XmlDocument xmlDocument = new XmlDocument();
					xmlDocument.LoadXml(wSManSession.Identify(0));
					base.WriteObject(xmlDocument.DocumentElement);
				}
				catch (Exception exception1)
				{
					try
					{
						if (!string.IsNullOrEmpty(wSManSession.Error))
						{
							XmlDocument xmlDocument1 = new XmlDocument();
							xmlDocument1.LoadXml(wSManSession.Error);
							InvalidOperationException invalidOperationException = new InvalidOperationException(xmlDocument1.OuterXml);
							ErrorRecord errorRecord = new ErrorRecord(invalidOperationException, "WsManError", ErrorCategory.InvalidOperation, this.computername);
							base.WriteError(errorRecord);
						}
					}
					catch (Exception exception)
					{
					}
				}
			}
			finally
			{
				if (wSManSession != null)
				{
					this.Dispose(wSManSession);
				}
			}
		}
	}
}