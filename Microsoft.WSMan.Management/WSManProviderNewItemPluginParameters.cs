using System;
using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;

namespace Microsoft.WSMan.Management
{
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Plugin")]
	public class WSManProviderNewItemPluginParameters
	{
		private string _plugin;

		private string _filename;

		private string _sdkversion;

		private Uri _resourceuri;

		private object[] _capability;

		private string _xmlRenderingtype;

		private string _file;

		private PSCredential runAsCredentials;

		private bool sharedHost;

		private bool autoRestart;

		private uint? processIdleTimeoutSeconds;

		[Parameter]
		public SwitchParameter AutoRestart
		{
			get
			{
				return this.autoRestart;
			}
			set
			{
				this.autoRestart = value;
			}
		}

		[Parameter(Mandatory=true, ParameterSetName="pathSet")]
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		[ValidateNotNullOrEmpty]
		public object[] Capability
		{
			get
			{
				return this._capability;
			}
			set
			{
				this._capability = value;
			}
		}

		[Parameter(Mandatory=true, ParameterSetName="nameSet")]
		[ValidateNotNullOrEmpty]
		public string File
		{
			get
			{
				return this._file;
			}
			set
			{
				this._file = value;
			}
		}

		[Parameter(Mandatory=true, ParameterSetName="pathSet")]
		[ValidateNotNullOrEmpty]
		public string FileName
		{
			get
			{
				return this._filename;
			}
			set
			{
				this._filename = value;
			}
		}

		[Parameter(Mandatory=true, ParameterSetName="pathSet")]
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Plugin")]
		[ValidateNotNullOrEmpty]
		public string Plugin
		{
			get
			{
				return this._plugin;
			}
			set
			{
				this._plugin = value;
			}
		}

		[Parameter]
		public uint? ProcessIdleTimeoutSec
		{
			get
			{
				return this.processIdleTimeoutSeconds;
			}
			set
			{
				this.processIdleTimeoutSeconds = value;
			}
		}

		[Parameter(Mandatory=true, ParameterSetName="pathSet")]
		[ValidateNotNullOrEmpty]
		public Uri Resource
		{
			get
			{
				return this._resourceuri;
			}
			set
			{
				this._resourceuri = value;
			}
		}

		[Parameter]
		[ValidateNotNull]
		public PSCredential RunAsCredential
		{
			get
			{
				return this.runAsCredentials;
			}
			set
			{
				this.runAsCredentials = value;
			}
		}

		[Parameter(Mandatory=true, ParameterSetName="pathSet")]
		[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId="SDK")]
		[ValidateNotNullOrEmpty]
		public string SDKVersion
		{
			get
			{
				return this._sdkversion;
			}
			set
			{
				this._sdkversion = value;
			}
		}

		[Parameter]
		public SwitchParameter UseSharedProcess
		{
			get
			{
				return this.sharedHost;
			}
			set
			{
				this.sharedHost = value;
			}
		}

		[Parameter(Mandatory=true, ParameterSetName="pathSet")]
		[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId="XML")]
		[ValidateNotNullOrEmpty]
		public string XMLRenderingType
		{
			get
			{
				return this._xmlRenderingtype;
			}
			set
			{
				this._xmlRenderingtype = value;
			}
		}

		public WSManProviderNewItemPluginParameters()
		{
		}
	}
}