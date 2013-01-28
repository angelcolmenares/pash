using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;
using System.Xml;

namespace Microsoft.WSMan.Management
{
	[Cmdlet("Get", "WSManInstance", DefaultParameterSetName="GetInstance", HelpUri="http://go.microsoft.com/fwlink/?LinkId=141444")]
	public class GetWSManInstanceCommand : AuthenticatingWSManCommand, IDisposable
	{
		private string applicationname;

		private SwitchParameter basepropertiesonly;

		private string computername;

		private Uri connectionuri;

		private Uri dialect;

		private SwitchParameter enumerate;

		private string filter;

		private string fragment;

		private Hashtable optionset;

		private int port;

		private SwitchParameter associations;

		private Uri resourceuri;

		private string returntype;

		private Hashtable selectorset;

		private SessionOption sessionoption;

		private SwitchParameter shallow;

		private SwitchParameter usessl;

		private WSManHelper helper;

		[Parameter(ParameterSetName="GetInstance")]
		[Parameter(ParameterSetName="Enumerate")]
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

		[Parameter(ParameterSetName="Enumerate")]
		public SwitchParameter Associations
		{
			get
			{
				return this.associations;
			}
			set
			{
				this.associations = value;
			}
		}

		[Alias(new string[] { "UBPO", "Base" })]
		[Parameter(ParameterSetName="Enumerate")]
		public SwitchParameter BasePropertiesOnly
		{
			get
			{
				return this.basepropertiesonly;
			}
			set
			{
				this.basepropertiesonly = value;
			}
		}

		[Alias(new string[] { "CN" })]
		[Parameter(ParameterSetName="GetInstance")]
		[Parameter(ParameterSetName="Enumerate")]
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
		[Parameter(ParameterSetName="GetInstance")]
		[Parameter(ParameterSetName="Enumerate")]
		[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId="URI")]
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

		[Parameter(Mandatory=true, ParameterSetName="Enumerate")]
		public SwitchParameter Enumerate
		{
			get
			{
				return this.enumerate;
			}
			set
			{
				this.enumerate = value;
			}
		}

		[Parameter(ParameterSetName="Enumerate")]
		[ValidateNotNullOrEmpty]
		public string Filter
		{
			get
			{
				return this.filter;
			}
			set
			{
				this.filter = value;
			}
		}

		[Parameter(ParameterSetName="GetInstance")]
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

		[Alias(new string[] { "OS" })]
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

		[Parameter(ParameterSetName="GetInstance")]
		[Parameter(ParameterSetName="Enumerate")]
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

		[Alias(new string[] { "RURI" })]
		[Parameter(Mandatory=true, Position=0, ValueFromPipeline=true, ValueFromPipelineByPropertyName=true)]
		[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId="URI")]
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

		[Alias(new string[] { "RT" })]
		[Parameter(ParameterSetName="Enumerate")]
		[ValidateNotNullOrEmpty]
		[ValidateSet(new string[] { "object", "epr", "objectandepr" })]
		public string ReturnType
		{
			get
			{
				return this.returntype;
			}
			set
			{
				this.returntype = value;
			}
		}

		[Parameter(ParameterSetName="GetInstance")]
		[SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
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

		[Alias(new string[] { "SO" })]
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

		[Parameter(ParameterSetName="Enumerate")]
		public SwitchParameter Shallow
		{
			get
			{
				return this.shallow;
			}
			set
			{
				this.shallow = value;
			}
		}

		[Alias(new string[] { "SSL" })]
		[Parameter(ParameterSetName="Enumerate")]
		[Parameter(ParameterSetName="GetInstance")]
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

		public GetWSManInstanceCommand()
		{
			this.returntype = "object";
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

		private string GetFilter()
		{
			char[] chrArray = new char[2];
			chrArray[0] = '=';
			chrArray[1] = ';';
			string[] strArrays = this.filter.Trim().Split(chrArray);
			if ((int)strArrays.Length % 2 == 0)
			{
				this.filter = "<wsman:SelectorSet xmlns:wsman='http://schemas.dmtf.org/wbem/wsman/1/wsman.xsd'>";
				for (int i = 0; i < (int)strArrays.Length; i = i + 2)
				{
					string str = strArrays[i + 1].Substring(1, strArrays[i + 1].Length - 2);
					string str1 = strArrays[i];
					string[] strArrays1 = new string[6];
					strArrays1[0] = this.filter;
					strArrays1[1] = "<wsman:Selector Name='";
					strArrays1[2] = str1;
					strArrays1[3] = "'>";
					strArrays1[4] = str;
					strArrays1[5] = "</wsman:Selector>";
					this.filter = string.Concat(strArrays1);
				}
				this.filter = string.Concat(this.filter, "</wsman:SelectorSet>");
				return this.filter.ToString();
			}
			else
			{
				return null;
			}
		}

		protected override void ProcessRecord()
		{
			IWSManSession wSManSession = null;
			IWSManEx wSManClass = (IWSManEx)(new WSManClass());
			this.helper = new WSManHelper(this);
			this.helper.WSManOp = "Get";
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
			try
			{
				IWSManResourceLocator wSManResourceLocator = this.helper.InitializeResourceLocator(this.optionset, this.selectorset, this.fragment, this.dialect, wSManClass, this.resourceuri);
				wSManSession = this.helper.CreateSessionObject(wSManClass, this.Authentication, this.sessionoption, this.Credential, str, this.CertificateThumbprint, this.usessl.IsPresent);
				if (this.enumerate)
				{
					try
					{
						this.ReturnEnumeration(wSManClass, wSManResourceLocator, wSManSession);
					}
					catch (Exception exception1)
					{
						Exception exception = exception1;
						this.helper.AssertError(exception.Message, false, this.computername);
					}
				}
				else
				{
					XmlDocument xmlDocument = new XmlDocument();
					try
					{
						xmlDocument.LoadXml(wSManSession.Get(wSManResourceLocator, 0));
					}
					catch (XmlException xmlException1)
					{
						XmlException xmlException = xmlException1;
						this.helper.AssertError(xmlException.Message, false, this.computername);
					}
					if (string.IsNullOrEmpty(this.fragment))
					{
						base.WriteObject(xmlDocument.FirstChild);
					}
					else
					{
						base.WriteObject(string.Concat(xmlDocument.FirstChild.LocalName, "=", xmlDocument.FirstChild.InnerText));
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

		private void ReturnEnumeration(IWSManEx wsmanObject, IWSManResourceLocator wsmanResourceLocator, IWSManSession wsmanSession)
		{
			string uRIWQLDIALECT;
			IWSManEnumerator wSManEnumerator;
			try
			{
				int num = 0;
				if (this.returntype != null)
				{
					if (!this.returntype.Equals("object", StringComparison.CurrentCultureIgnoreCase))
					{
						if (!this.returntype.Equals("epr", StringComparison.CurrentCultureIgnoreCase))
						{
							num = wsmanObject.EnumerationFlagReturnObjectAndEPR();
						}
						else
						{
							num = wsmanObject.EnumerationFlagReturnEPR();
						}
					}
					else
					{
						num = wsmanObject.EnumerationFlagReturnObject();
					}
				}
				if (!this.shallow)
				{
					if (!this.basepropertiesonly)
					{
						num = num | wsmanObject.EnumerationFlagHierarchyDeep();
					}
					else
					{
						num = num | wsmanObject.EnumerationFlagHierarchyDeepBasePropsOnly();
					}
				}
				else
				{
					num = num | wsmanObject.EnumerationFlagHierarchyShallow();
				}
				if (!(this.dialect != null) || this.filter == null)
				{
					if (this.filter == null)
					{
						wSManEnumerator = (IWSManEnumerator)wsmanSession.Enumerate(wsmanResourceLocator, this.filter, null, num);
					}
					else
					{
						uRIWQLDIALECT = this.helper.URI_WQL_DIALECT;
						this.dialect = new Uri(uRIWQLDIALECT);
						wSManEnumerator = (IWSManEnumerator)wsmanSession.Enumerate(wsmanResourceLocator, this.filter, this.dialect.ToString(), num);
					}
				}
				else
				{
					if (this.dialect.ToString().Equals(this.helper.ALIAS_WQL, StringComparison.CurrentCultureIgnoreCase) || this.dialect.ToString().Equals(this.helper.URI_WQL_DIALECT, StringComparison.CurrentCultureIgnoreCase))
					{
						uRIWQLDIALECT = this.helper.URI_WQL_DIALECT;
						this.dialect = new Uri(uRIWQLDIALECT);
					}
					else
					{
						if (this.dialect.ToString().Equals(this.helper.ALIAS_ASSOCIATION, StringComparison.CurrentCultureIgnoreCase) || this.dialect.ToString().Equals(this.helper.URI_ASSOCIATION_DIALECT, StringComparison.CurrentCultureIgnoreCase))
						{
							if (!this.associations)
							{
								num = num | wsmanObject.EnumerationFlagAssociatedInstance();
							}
							else
							{
								num = num | wsmanObject.EnumerationFlagAssociationInstance();
							}
							uRIWQLDIALECT = this.helper.URI_ASSOCIATION_DIALECT;
							this.dialect = new Uri(uRIWQLDIALECT);
						}
						else
						{
							if (this.dialect.ToString().Equals(this.helper.ALIAS_SELECTOR, StringComparison.CurrentCultureIgnoreCase) || this.dialect.ToString().Equals(this.helper.URI_SELECTOR_DIALECT, StringComparison.CurrentCultureIgnoreCase))
							{
								this.filter = this.GetFilter();
								uRIWQLDIALECT = this.helper.URI_SELECTOR_DIALECT;
								this.dialect = new Uri(uRIWQLDIALECT);
							}
						}
					}
					wSManEnumerator = (IWSManEnumerator)wsmanSession.Enumerate(wsmanResourceLocator, this.filter, this.dialect.ToString(), num);
				}
				while (!wSManEnumerator.AtEndOfStream)
				{
					XmlDocument xmlDocument = new XmlDocument();
					xmlDocument.LoadXml(wSManEnumerator.ReadItem());
					base.WriteObject(xmlDocument.FirstChild);
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				ErrorRecord errorRecord = new ErrorRecord(exception, "Exception", ErrorCategory.InvalidOperation, null);
				base.WriteError(errorRecord);
			}
		}
	}
}