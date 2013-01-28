using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace Microsoft.WSMan.Management
{
	public sealed class SessionOption
	{
		private bool _SkipCACheck;

		private bool _SkipCNCheck;

		private bool _SkipRevocationCheck;

		private bool _useencryption;

		private bool _UTF16;

		private ProxyAuthentication _ProxyAuthentication;

		private int _SPNPort;

		private int _OperationTimeout;

		private NetworkCredential _ProxyCredential;

		private ProxyAccessType _proxyaccesstype;

		public int OperationTimeout
		{
			get
			{
				return this._OperationTimeout;
			}
			set
			{
				this._OperationTimeout = value;
			}
		}

		public ProxyAccessType ProxyAccessType
		{
			get
			{
				return this._proxyaccesstype;
			}
			set
			{
				this._proxyaccesstype = value;
			}
		}

		public ProxyAuthentication ProxyAuthentication
		{
			get
			{
				return this._ProxyAuthentication;
			}
			set
			{
				this._ProxyAuthentication = value;
			}
		}

		public NetworkCredential ProxyCredential
		{
			get
			{
				return this._ProxyCredential;
			}
			set
			{
				this._ProxyCredential = value;
			}
		}

		public bool SkipCACheck
		{
			get
			{
				return this._SkipCACheck;
			}
			set
			{
				this._SkipCACheck = value;
			}
		}

		public bool SkipCNCheck
		{
			get
			{
				return this._SkipCNCheck;
			}
			set
			{
				this._SkipCNCheck = value;
			}
		}

		public bool SkipRevocationCheck
		{
			get
			{
				return this._SkipRevocationCheck;
			}
			set
			{
				this._SkipRevocationCheck = value;
			}
		}

		[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId="SPN")]
		public int SPNPort
		{
			get
			{
				return this._SPNPort;
			}
			set
			{
				this._SPNPort = value;
			}
		}

		public bool UseEncryption
		{
			get
			{
				return this._useencryption;
			}
			set
			{
				this._useencryption = value;
			}
		}

		public bool UseUtf16
		{
			get
			{
				return this._UTF16;
			}
			set
			{
				this._UTF16 = value;
			}
		}

		public SessionOption()
		{
			this._useencryption = true;
		}
	}
}