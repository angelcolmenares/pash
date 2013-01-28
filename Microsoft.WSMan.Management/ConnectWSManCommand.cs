using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;

namespace Microsoft.WSMan.Management
{
	[Cmdlet("Connect", "WSMan", DefaultParameterSetName="ComputerName", HelpUri="http://go.microsoft.com/fwlink/?LinkId=141437")]
	public class ConnectWSManCommand : AuthenticatingWSManCommand
	{
		private string applicationname;

		private string computername;

		private Uri connectionuri;

		private Hashtable optionset;

		private int port;

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
		[Parameter(ParameterSetName="ComputerName", Position=0)]
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

		[Parameter]
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

		public ConnectWSManCommand()
		{
		}

		protected override void BeginProcessing()
		{
			WSManHelper wSManHelper = new WSManHelper(this);
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
			string str = this.computername;
			if (str == null)
			{
				str = "localhost";
			}
			object[] name = new object[4];
			name[0] = base.SessionState.Drive.Current.Name;
			name[1] = ":";
			name[2] = (char)92;
			name[3] = str;
			if (base.SessionState.Path.CurrentProviderLocation("WSMan").Path.StartsWith(string.Concat(name), StringComparison.CurrentCultureIgnoreCase))
			{
				wSManHelper.AssertError(wSManHelper.GetResourceMsgFromResourcetext("ConnectFailure"), false, this.computername);
			}
			wSManHelper.CreateWsManConnection(base.ParameterSetName, this.connectionuri, this.port, this.computername, this.applicationname, this.usessl.IsPresent, this.Authentication, this.sessionoption, this.Credential, this.CertificateThumbprint);
		}
	}
}