using Microsoft.ActiveDirectory.Management;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class MoveADDirectoryServerOperationMasterRoleParameterSet : ADParameterSet
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

		[Parameter]
		[ValidateNotNull]
		[ValidateNotNullOrEmpty]
		public SwitchParameter Force
		{
			get
			{
				return base.GetSwitchParameter("Force");
			}
			set
			{
				base["Force"] = value;
			}
		}

		[Parameter(Mandatory=true, Position=0, ValueFromPipeline=true)]
		[ValidateNotNull]
		public ADDirectoryServer Identity
		{
			get
			{
				return base["Identity"] as ADDirectoryServer;
			}
			set
			{
				base["Identity"] = value;
			}
		}

		[Parameter(Mandatory=true, Position=1)]
		[ValidateCollectionIsUnique]
		[ValidateCount(0, 5)]
		[ValidateNotNull]
		public ADOperationMasterRole[] OperationMasterRole
		{
			get
			{
				return base["OperationMasterRole"] as ADOperationMasterRole[];
			}
			set
			{
				base["OperationMasterRole"] = value;
			}
		}

		[Parameter]
		[ValidateNotNull]
		[ValidateNotNullOrEmpty]
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

		[Parameter]
		[ValidateNotNullOrEmpty]
		public string Server
		{
			get
			{
				return base["Server"] as string;
			}
			set
			{
				base["Server"] = value;
			}
		}

		public MoveADDirectoryServerOperationMasterRoleParameterSet()
		{
		}
	}
}