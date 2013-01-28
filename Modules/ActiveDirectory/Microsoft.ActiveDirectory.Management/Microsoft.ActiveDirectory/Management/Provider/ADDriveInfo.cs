using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Provider
{
	public class ADDriveInfo : PSDriveInfo
	{
		private string _server;

		private ADPathFormat _formatType;

		private ADAuthType _authType;

		private bool _isGC;

		private bool _ssl;

		private bool _encryption;

		private bool _signing;

		private ADSessionInfo _sessionInfo;

		private ADSession _session;

		private string _partitionDN;

		private HashSet<string> _namingContexts;

		private string _rootWithoutAbsolutePathToken;

		public ADAuthType AuthType
		{
			get
			{
				return this._authType;
			}
			set
			{
				this._authType = value;
			}
		}

		public bool Encryption
		{
			get
			{
				return this._encryption;
			}
			set
			{
				this._encryption = value;
			}
		}

		public ADPathFormat FormatType
		{
			get
			{
				return this._formatType;
			}
			set
			{
				this._formatType = value;
			}
		}

		public bool GlobalCatalog
		{
			get
			{
				return this._isGC;
			}
			set
			{
				this._isGC = value;
			}
		}

		internal HashSet<string> NamingContexts
		{
			get
			{
				return this._namingContexts;
			}
			set
			{
				this._namingContexts = value;
			}
		}

		internal string RootPartitionPath
		{
			get
			{
				return this._partitionDN;
			}
			set
			{
				this._partitionDN = value;
			}
		}

		public string RootWithoutAbsolutePathToken
		{
			get
			{
				return this._rootWithoutAbsolutePathToken;
			}
		}

		public bool SecureSocketLayer
		{
			get
			{
				return this._ssl;
			}
			set
			{
				this._ssl = value;
			}
		}

		public string Server
		{
			get
			{
				return this._server;
			}
			set
			{
				this._server = value;
			}
		}

		internal ADSession Session
		{
			get
			{
				return this._session;
			}
			set
			{
				this._session = value;
			}
		}

		internal ADSessionInfo SessionInfo
		{
			get
			{
				return this._sessionInfo;
			}
			set
			{
				this._sessionInfo = value;
			}
		}

		public bool Signing
		{
			get
			{
				return this._signing;
			}
			set
			{
				this._signing = value;
			}
		}

		public ADDriveInfo(PSDriveInfo driveInfo) : base(driveInfo)
		{
			this._formatType = ADProviderDefaults.PathFormat;
			this._authType = ADProviderDefaults.AuthType;
			this._isGC = ADProviderDefaults.IsGC;
			this._ssl = ADProviderDefaults.Ssl;
			this._encryption = ADProviderDefaults.Encryption;
			this._signing = ADProviderDefaults.Signing;
		}

		public ADDriveInfo(string name, ProviderInfo provider, string root, string rootWithAbsoluteToken, string description, PSCredential credential) : base(name, provider, rootWithAbsoluteToken, description, credential)
		{
			this._formatType = ADProviderDefaults.PathFormat;
			this._authType = ADProviderDefaults.AuthType;
			this._isGC = ADProviderDefaults.IsGC;
			this._ssl = ADProviderDefaults.Ssl;
			this._encryption = ADProviderDefaults.Encryption;
			this._signing = ADProviderDefaults.Signing;
			this._rootWithoutAbsolutePathToken = root;
		}

		public ADDriveInfo() : base(null)
		{
			this._formatType = ADProviderDefaults.PathFormat;
			this._authType = ADProviderDefaults.AuthType;
			this._isGC = ADProviderDefaults.IsGC;
			this._ssl = ADProviderDefaults.Ssl;
			this._encryption = ADProviderDefaults.Encryption;
			this._signing = ADProviderDefaults.Signing;
		}
	}
}