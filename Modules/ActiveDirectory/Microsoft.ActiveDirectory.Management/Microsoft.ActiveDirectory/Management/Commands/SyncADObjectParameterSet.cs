using Microsoft.ActiveDirectory.Management;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class SyncADObjectParameterSet : ADParameterSet
	{
		[Parameter]
		public ADAuthType AuthType
		{
			get
			{
				return (ADAuthType)base["AuthType"];
			}
			set
			{
				base["AuthType"] = value;
			}
		}

		[Credential]
		[Parameter]
		[ValidateNotNullOrEmpty]
		public PSCredential Credential
		{
			get
			{
				return base["Credential"] as PSCredential;
			}
			set
			{
				base["Credential"] = value;
			}
		}

		[Alias(new string[] { "Server", "HostName", "IPv4Address" })]
		[Parameter(Mandatory=true, Position=2, ParameterSetName="Object")]
		[ValidateNotNullOrEmpty]
		public string Destination
		{
			get
			{
				return base["Destination"] as string;
			}
			set
			{
				base["Destination"] = value;
			}
		}

		[Parameter(Mandatory=true, Position=0, ValueFromPipeline=true, ParameterSetName="Object")]
		[ValidateNotNull]
		[ValidateNotNullOrEmptyADEntity]
		public ADObject Object
		{
			get
			{
				return base["Object"] as ADObject;
			}
			set
			{
				base["Object"] = value;
			}
		}

		[Parameter]
		[ValidateNotNull]
		public SwitchParameter PassThru
		{
			get
			{
				return base.GetSwitchParameter("PassThru");
			}
			set
			{
				base["PassThru"] = value;
			}
		}

		[Parameter(ParameterSetName="Object")]
		[ValidateNotNull]
		public SwitchParameter PasswordOnly
		{
			get
			{
				return base.GetSwitchParameter("PasswordOnly");
			}
			set
			{
				base["PasswordOnly"] = value;
			}
		}

		[Parameter(Position=1, ParameterSetName="Object")]
		[ValidateNotNullOrEmpty]
		public string Source
		{
			get
			{
				return base["Source"] as string;
			}
			set
			{
				base["Source"] = value;
			}
		}

		public SyncADObjectParameterSet()
		{
		}
	}
}