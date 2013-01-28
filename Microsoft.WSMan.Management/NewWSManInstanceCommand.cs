using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;
using System.Xml;

namespace Microsoft.WSMan.Management
{
	[Cmdlet("New", "WSManInstance", DefaultParameterSetName="ComputerName", HelpUri="http://go.microsoft.com/fwlink/?LinkId=141448")]
	public class NewWSManInstanceCommand : AuthenticatingWSManCommand, IDisposable
	{
		private string applicationname;

		private string computername;

		private Uri connectionuri;

		private string filepath;

		private Hashtable optionset;

		private int port;

		private Uri resourceuri;

		private Hashtable selectorset;

		private SessionOption sessionoption;

		private SwitchParameter usessl;

		private Hashtable valueset;

		private WSManHelper helper;

		private IWSManEx m_wsmanObject;

		private IWSManSession m_session;

		private string connectionStr;

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

		[Parameter(Mandatory=true, Position=1, ValueFromPipeline=true)]
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

		public NewWSManInstanceCommand()
		{
			this.m_wsmanObject = (IWSManEx)(new WSManClass());
			this.connectionStr = string.Empty;
		}

		protected override void BeginProcessing()
		{
			this.helper = new WSManHelper(this);
			this.helper.WSManOp = "new";
			this.connectionStr = this.helper.CreateConnectionString(this.connectionuri, this.port, this.computername, this.applicationname);
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
					this.helper.AssertError(this.helper.GetResourceMsgFromResourcetext("NotProperURI"), false, this.connectionuri);
				}
			}
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
				string rootNodeName = this.helper.GetRootNodeName(this.helper.WSManOp, wSManResourceLocator.resourceUri, null);
				string str = this.helper.ProcessInput(this.m_wsmanObject, this.filepath, this.helper.WSManOp, rootNodeName, this.valueset, wSManResourceLocator, this.m_session);
				try
				{
					string str1 = this.m_session.Create(wSManResourceLocator, str, 0);
					XmlDocument xmlDocument = new XmlDocument();
					xmlDocument.LoadXml(str1);
					base.WriteObject(xmlDocument.DocumentElement);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					this.helper.AssertError(exception.Message, false, this.computername);
				}
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