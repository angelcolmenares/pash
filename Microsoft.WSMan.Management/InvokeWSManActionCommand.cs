using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;
using System.Xml;

namespace Microsoft.WSMan.Management
{
	[Cmdlet("Invoke", "WSManAction", DefaultParameterSetName="URI", HelpUri="http://go.microsoft.com/fwlink/?LinkId=141446")]
	public class InvokeWSManActionCommand : AuthenticatingWSManCommand, IDisposable
	{
		private string action;

		private string applicationname;

		private string computername;

		private Uri connectionuri;

		private string filepath;

		private Hashtable optionset;

		private int port;

		private Hashtable selectorset;

		private SessionOption sessionoption;

		private SwitchParameter usessl;

		private Hashtable valueset;

		private Uri resourceuri;

		private WSManHelper helper;

		private IWSManEx m_wsmanObject;

		private IWSManSession m_session;

		private string connectionStr;

		[Parameter(Mandatory=true, Position=1)]
		[ValidateNotNullOrEmpty]
		public string Action
		{
			get
			{
				return this.action;
			}
			set
			{
				this.action = value;
			}
		}

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

		[Alias(new string[] { "CURI", "CU" })]
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

		[Parameter]
		[ValidateNotNullOrEmpty]
		public string FilePath
		{
			get
			{
				return this.filepath;
			}
			set
			{
				this.filepath = value;
			}
		}

		[Alias(new string[] { "os" })]
		[Parameter(ValueFromPipeline=true, ValueFromPipelineByPropertyName=true)]
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
		[Parameter(Mandatory=true, Position=0, ValueFromPipeline=true, ValueFromPipelineByPropertyName=true)]
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

		[Parameter(Position=2, ValueFromPipeline=true, ValueFromPipelineByPropertyName=true)]
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

		[Parameter]
		[SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
		[ValidateNotNullOrEmpty]
		public Hashtable ValueSet
		{
			get
			{
				return this.valueset;
			}
			set
			{
				this.valueset = value;
			}
		}

		public InvokeWSManActionCommand()
		{
			this.m_wsmanObject = (IWSManEx)(new WSManClass());
			this.connectionStr = string.Empty;
		}

		protected override void BeginProcessing()
		{
			this.helper = new WSManHelper(this);
			this.helper.WSManOp = "invoke";
			this.connectionStr = this.helper.CreateConnectionString(this.connectionuri, this.port, this.computername, this.applicationname);
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

		protected override void EndProcessing()
		{
			this.helper.CleanUp();
		}

		protected override void ProcessRecord()
		{
			try
			{
				IWSManResourceLocator wSManResourceLocator = this.helper.InitializeResourceLocator(this.optionset, this.selectorset, null, null, this.m_wsmanObject, this.resourceuri);
				this.m_session = this.helper.CreateSessionObject(this.m_wsmanObject, this.Authentication, this.sessionoption, this.Credential, this.connectionStr, this.CertificateThumbprint, this.usessl.IsPresent);
				string rootNodeName = this.helper.GetRootNodeName(this.helper.WSManOp, wSManResourceLocator.resourceUri, this.action);
				string str = this.helper.ProcessInput(this.m_wsmanObject, this.filepath, this.helper.WSManOp, rootNodeName, this.valueset, wSManResourceLocator, this.m_session);
				string str1 = this.m_session.Invoke(this.action, wSManResourceLocator, str, 0);
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(str1);
				base.WriteObject(xmlDocument.DocumentElement);
			}
			finally
			{
				if (!string.IsNullOrEmpty(this.m_wsmanObject.Error))
				{
					this.helper.AssertError(this.m_wsmanObject.Error, true, this.resourceuri);
				}
				if (!string.IsNullOrEmpty(this.m_session.Error))
				{
					this.helper.AssertError(this.m_session.Error, true, this.resourceuri);
				}
				if (this.m_session != null)
				{
					this.Dispose(this.m_session);
				}
			}
		}
	}
}