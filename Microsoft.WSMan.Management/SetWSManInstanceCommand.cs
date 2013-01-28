using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;
using System.Xml;

namespace Microsoft.WSMan.Management
{
	[Cmdlet("Set", "WSManInstance", DefaultParameterSetName="ComputerName", HelpUri="http://go.microsoft.com/fwlink/?LinkId=141458")]
	public class SetWSManInstanceCommand : AuthenticatingWSManCommand, IDisposable
	{
		private string applicationname;

		private string computername;

		private Uri connectionuri;

		private Uri dialect;

		private string filepath;

		private string fragment;

		private Hashtable optionset;

		private int port;

		private Uri resourceuri;

		private Hashtable selectorset;

		private SessionOption sessionoption;

		private SwitchParameter usessl;

		private Hashtable valueset;

		private WSManHelper helper;

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

		[Parameter]
		[ValidateNotNullOrEmpty]
		public Uri Dialect
		{
			get
			{
				return this.dialect;
			}
			set
			{
				this.dialect = value;
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
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

		[Parameter(ParameterSetName="ComputerName")]
		[Parameter(ParameterSetName="URI")]
		[ValidateNotNullOrEmpty]
		public string Fragment
		{
			get
			{
				return this.fragment;
			}
			set
			{
				this.fragment = value;
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
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Resourceuri")]
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

		[Parameter(Position=1, ValueFromPipeline=true, ValueFromPipelineByPropertyName=true)]
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

		[Parameter(ValueFromPipelineByPropertyName=true)]
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

		public SetWSManInstanceCommand()
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

		protected override void EndProcessing()
		{
			this.helper.CleanUp();
		}

		protected override void ProcessRecord()
		{
			IWSManEx wSManClass = (IWSManEx)(new WSManClass());
			this.helper = new WSManHelper(this);
			this.helper.WSManOp = "set";
			IWSManSession wSManSession = null;
			if (this.dialect != null)
			{
				if (this.dialect.ToString().Equals(this.helper.ALIAS_WQL, StringComparison.CurrentCultureIgnoreCase))
				{
					this.dialect = new Uri(this.helper.URI_WQL_DIALECT);
				}
				if (this.dialect.ToString().Equals(this.helper.ALIAS_SELECTOR, StringComparison.CurrentCultureIgnoreCase))
				{
					this.dialect = new Uri(this.helper.URI_SELECTOR_DIALECT);
				}
				if (this.dialect.ToString().Equals(this.helper.ALIAS_ASSOCIATION, StringComparison.CurrentCultureIgnoreCase))
				{
					this.dialect = new Uri(this.helper.URI_ASSOCIATION_DIALECT);
				}
			}
			try
			{
				string str = this.helper.CreateConnectionString(this.connectionuri, this.port, this.computername, this.applicationname);
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
				IWSManResourceLocator wSManResourceLocator = this.helper.InitializeResourceLocator(this.optionset, this.selectorset, this.fragment, this.dialect, wSManClass, this.resourceuri);
				wSManSession = this.helper.CreateSessionObject(wSManClass, this.Authentication, this.sessionoption, this.Credential, str, this.CertificateThumbprint, this.usessl.IsPresent);
				string rootNodeName = this.helper.GetRootNodeName(this.helper.WSManOp, wSManResourceLocator.resourceUri, null);
				string str1 = this.helper.ProcessInput(wSManClass, this.filepath, this.helper.WSManOp, rootNodeName, this.valueset, wSManResourceLocator, wSManSession);
				XmlDocument xmlDocument = new XmlDocument();
				try
				{
					xmlDocument.LoadXml(wSManSession.Put(wSManResourceLocator, str1, 0));
				}
				catch (XmlException xmlException1)
				{
					XmlException xmlException = xmlException1;
					this.helper.AssertError(xmlException.Message, false, this.computername);
				}
				if (string.IsNullOrEmpty(this.fragment))
				{
					base.WriteObject(xmlDocument.DocumentElement);
				}
				else
				{
					if (xmlDocument.DocumentElement.ChildNodes.Count > 0)
					{
						foreach (XmlNode childNode in xmlDocument.DocumentElement.ChildNodes)
						{
							if (!childNode.Name.Equals(this.fragment, StringComparison.CurrentCultureIgnoreCase))
							{
								continue;
							}
							base.WriteObject(string.Concat(childNode.Name, " = ", childNode.InnerText));
						}
					}
				}
			}
			finally
			{
				if (!string.IsNullOrEmpty(wSManClass.Error))
				{
					this.helper.AssertError(wSManClass.Error, true, this.resourceuri);
				}
				if (!string.IsNullOrEmpty(wSManSession.Error))
				{
					this.helper.AssertError(wSManSession.Error, true, this.resourceuri);
				}
				if (wSManSession != null)
				{
					this.Dispose(wSManSession);
				}
			}
		}
	}
}