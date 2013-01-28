using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;

namespace Microsoft.WSMan.Management
{
	[Cmdlet("Remove", "WSManInstance", DefaultParameterSetName="ComputerName", HelpUri="http://go.microsoft.com/fwlink/?LinkId=141453")]
	public class RemoveWSManInstanceCommand : AuthenticatingWSManCommand, IDisposable
	{
		private string applicationname;

		private string computername;

		private Uri connectionuri;

		private Hashtable optionset;

		private int port;

		private Uri resourceuri;

		private Hashtable selectorset;

		private SessionOption sessionoption;

		private SwitchParameter usessl;

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

		[Alias(new string[] { "cn" })]
		[Parameter(ParameterSetName="ComputerName")]
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

		[Parameter(ParameterSetName="URI")]
		[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId="URI")]
		[ValidateNotNullOrEmpty]
		public Uri ConnectionURI
		{
			get
			{
				return this.connectionuri;
			}
			set
			{
				this.connectionuri = value;
			}
		}

		[Alias(new string[] { "os" })]
		[Parameter]
		[SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
		[ValidateNotNullOrEmpty]
		public Hashtable OptionSet
		{
			get
			{
				return this.optionset;
			}
			set
			{
				this.optionset = value;
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

		[Alias(new string[] { "ruri" })]
		[Parameter(Mandatory=true, Position=0)]
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Resourceuri")]
		[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId="URI")]
		[ValidateNotNullOrEmpty]
		public Uri ResourceURI
		{
			get
			{
				return this.resourceuri;
			}
			set
			{
				this.resourceuri = value;
			}
		}

		[Parameter(Position=1, Mandatory=true, ValueFromPipeline=true, ValueFromPipelineByPropertyName=true)]
		[SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
		[ValidateNotNullOrEmpty]
		public Hashtable SelectorSet
		{
			get
			{
				return this.selectorset;
			}
			set
			{
				this.selectorset = value;
			}
		}

		[Alias(new string[] { "so" })]
		[Parameter]
		[SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
		[ValidateNotNullOrEmpty]
		public SessionOption SessionOption
		{
			get
			{
				return this.sessionoption;
			}
			set
			{
				this.sessionoption = value;
			}
		}

		[Alias(new string[] { "ssl" })]
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

		public RemoveWSManInstanceCommand()
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
			wSManHelper.WSManOp = "remove";
			IWSManSession wSManSession = null;
			try
			{
				string str = wSManHelper.CreateConnectionString(this.connectionuri, this.port, this.computername, this.applicationname);
				if (this.connectionuri != null)
				{
					try
					{
						string[] strArrays = new string[1];
						object[] objArray = new object[4];
						objArray[0] = ":";
						objArray[1] = this.port;
						objArray[2] = "/";
						objArray[3] = this.applicationname;
						strArrays[0] = string.Concat(objArray);
						string[] strArrays1 = this.connectionuri.OriginalString.Split(strArrays, StringSplitOptions.None);
						string[] strArrays2 = new string[1];
						strArrays2[0] = "//";
						string[] strArrays3 = strArrays1[0].Split(strArrays2, StringSplitOptions.None);
						this.computername = strArrays3[1].Trim();
					}
					catch (IndexOutOfRangeException indexOutOfRangeException)
					{
						wSManHelper.AssertError(wSManHelper.GetResourceMsgFromResourcetext("NotProperURI"), false, this.connectionuri);
					}
				}
				wSManHelper.InitializeResourceLocator(this.optionset, this.selectorset, null, null, wSManClass, this.resourceuri);
				wSManSession = wSManHelper.CreateSessionObject(wSManClass, this.Authentication, this.sessionoption, this.Credential, str, this.CertificateThumbprint, this.usessl.IsPresent);
				string uRIWithFilter = wSManHelper.GetURIWithFilter(this.resourceuri.ToString(), null, this.selectorset, wSManHelper.WSManOp);
				try
				{
					wSManSession.Delete(uRIWithFilter, 0);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					wSManHelper.AssertError(exception.Message, false, this.computername);
				}
			}
			finally
			{
				if (!string.IsNullOrEmpty(wSManSession.Error))
				{
					wSManHelper.AssertError(wSManSession.Error, true, this.resourceuri);
				}
				if (!string.IsNullOrEmpty(wSManClass.Error))
				{
					wSManHelper.AssertError(wSManClass.Error, true, this.resourceuri);
				}
				if (wSManSession != null)
				{
					this.Dispose(wSManSession);
				}
			}
		}
	}
}