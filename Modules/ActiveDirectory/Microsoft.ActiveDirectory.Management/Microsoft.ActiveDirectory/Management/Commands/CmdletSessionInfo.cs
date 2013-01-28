using Microsoft.ActiveDirectory.Management;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	internal class CmdletSessionInfo
	{
		private ADSessionInfo _adSessionInfo;

		private ADRootDSE _adRootDse;

		private string _defaultQueryPath;

		private string _defaultPartitionPath;

		private string _defaultCreationPath;

		private ADServerType _connectedADServerType;

		private IADCmdletCache _cmdletSessionCache;

		private IADCmdletMessageWriter _cmdletMessageWriter;

		private ADParameterSet _cmdletParameters;

		private PSCmdlet _psCmdlet;

		public ADRootDSE ADRootDSE
		{
			get
			{
				return this._adRootDse;
			}
			set
			{
				this._adRootDse = value;
			}
		}

		public ADSessionInfo ADSessionInfo
		{
			get
			{
				return this._adSessionInfo;
			}
			set
			{
				this._adSessionInfo = value;
			}
		}

		public PSCmdlet CmdletBase
		{
			get
			{
				return this._psCmdlet;
			}
			set
			{
				this._psCmdlet = value;
			}
		}

		public IADCmdletMessageWriter CmdletMessageWriter
		{
			get
			{
				return this._cmdletMessageWriter;
			}
			set
			{
				this._cmdletMessageWriter = value;
			}
		}

		public ADParameterSet CmdletParameters
		{
			get
			{
				return this._cmdletParameters;
			}
			set
			{
				this._cmdletParameters = value;
			}
		}

		public IADCmdletCache CmdletSessionCache
		{
			get
			{
				return this._cmdletSessionCache;
			}
			set
			{
				this._cmdletSessionCache = value;
			}
		}

		public ADServerType ConnectedADServerType
		{
			get
			{
				return this._connectedADServerType;
			}
			set
			{
				this._connectedADServerType = value;
			}
		}

		public string DefaultCreationPath
		{
			get
			{
				return this._defaultQueryPath;
			}
			set
			{
				this._defaultQueryPath = value;
			}
		}

		public string DefaultPartitionPath
		{
			get
			{
				return this._defaultPartitionPath;
			}
			set
			{
				this._defaultPartitionPath = value;
			}
		}

		public string DefaultQueryPath
		{
			get
			{
				return this._defaultQueryPath;
			}
			set
			{
				this._defaultQueryPath = value;
			}
		}

		public CmdletSessionInfo()
		{
		}

		public CmdletSessionInfo(ADSessionInfo adSessionInfo, ADRootDSE adRootDse, string defaultQueryPath, string defaultPartitionPath, string defaultCreationPath, ADServerType connectedADServerType, IADCmdletCache cmdletSessionCache, IADCmdletMessageWriter cmdletMessageWriter, PSCmdlet psCmdlet, ADParameterSet cmdletParameters)
		{
			this._adSessionInfo = adSessionInfo;
			this._adRootDse = adRootDse;
			this._defaultQueryPath = defaultQueryPath;
			this._defaultPartitionPath = defaultPartitionPath;
			this._defaultCreationPath = defaultCreationPath;
			this._connectedADServerType = connectedADServerType;
			this._cmdletSessionCache = cmdletSessionCache;
			this._cmdletMessageWriter = cmdletMessageWriter;
			this._psCmdlet = psCmdlet;
			this._cmdletParameters = cmdletParameters;
		}
	}
}