using System;
using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;

namespace Microsoft.WSMan.Management
{
	public class WSManProvidersListenerParameters
	{
		private string _address;

		private string _transport;

		private int _port;

		private string _hostName;

		private bool _enabled;

		private string _urlprefix;

		private string _certificatethumbprint;

		private bool _IsPortSpecified;

		[Parameter(Mandatory=true)]
		[ValidateNotNullOrEmpty]
		public string Address
		{
			get
			{
				return this._address;
			}
			set
			{
				this._address = value;
			}
		}

		[Parameter]
		[SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId="ThumbPrint")]
		[ValidateNotNullOrEmpty]
		public string CertificateThumbPrint
		{
			get
			{
				return this._certificatethumbprint;
			}
			set
			{
				this._certificatethumbprint = value;
			}
		}

		[Parameter]
		public bool Enabled
		{
			get
			{
				return this._enabled;
			}
			set
			{
				this._enabled = value;
			}
		}

		[Parameter]
		[ValidateNotNullOrEmpty]
		public string HostName
		{
			get
			{
				return this._hostName;
			}
			set
			{
				this._hostName = value;
			}
		}

		public bool IsPortSpecified
		{
			get
			{
				return this._IsPortSpecified;
			}
			set
			{
				this._IsPortSpecified = value;
			}
		}

		[Parameter]
		[ValidateNotNullOrEmpty]
		public int Port
		{
			get
			{
				return this._port;
			}
			set
			{
				this._port = value;
				this._IsPortSpecified = true;
			}
		}

		[Parameter(Mandatory=true)]
		[ValidateNotNullOrEmpty]
		public string Transport
		{
			get
			{
				return this._transport;
			}
			set
			{
				this._transport = value;
			}
		}

		[Parameter]
		[SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
		[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId="URL")]
		[ValidateNotNullOrEmpty]
		public string URLPrefix
		{
			get
			{
				return this._urlprefix;
			}
			set
			{
				this._urlprefix = value;
			}
		}

		public WSManProvidersListenerParameters()
		{
			this._transport = "http";
			this._enabled = true;
			this._urlprefix = "wsman";
		}
	}
}