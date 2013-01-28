using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;

namespace Microsoft.WSMan.Management
{
	public class WSManProviderNewItemComputerParameters
	{
		private Hashtable optionset;

		private AuthenticationMechanism authentication;

		private string thumbPrint;

		private SessionOption sessionoption;

		private string applicationname;

		private int port;

		private SwitchParameter usessl;

		private Uri connectionuri;

		[Parameter(ParameterSetName="nameSet")]
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

		[Parameter]
		[ValidateNotNullOrEmpty]
		public AuthenticationMechanism Authentication
		{
			get
			{
				return this.authentication;
			}
			set
			{
				this.authentication = value;
			}
		}

		[Parameter]
		[ValidateNotNullOrEmpty]
		public string CertificateThumbprint
		{
			get
			{
				return this.thumbPrint;
			}
			set
			{
				this.thumbPrint = value;
			}
		}

		[Parameter(ParameterSetName="pathSet", Mandatory=true)]
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

		[Alias(new string[] { "OS" })]
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

		[Parameter(ParameterSetName="nameSet")]
		[Parameter]
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

		[Parameter(ParameterSetName="nameSet")]
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

		public WSManProviderNewItemComputerParameters()
		{
			this.authentication = AuthenticationMechanism.Default;
			this.applicationname = "wsman";
		}
	}
}