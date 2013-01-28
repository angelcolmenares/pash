using Microsoft.ActiveDirectory.Management;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("New", "ADGroup", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219326", SupportsShouldProcess=true)]
	public class NewADGroup : ADNewCmdletBase<NewADGroupParameterSet, ADGroupFactory<ADGroup>, ADGroup>
	{
		private const string _debugCategory = "NewADGroup";

		public NewADGroup()
		{
		}

		protected internal override string GetDefaultCreationPathBase()
		{
			return Utils.GetWellKnownGuidDN(this.GetSessionInfo(), this.GetDefaultPartitionPath(), WellKnownGuids.UsersContainerGuid);
		}

		protected internal override void ValidateParameters()
		{
			base.ValidateParameters();
			CmdletSessionInfo cmdletSessionInfo = this.GetCmdletSessionInfo();
			if (cmdletSessionInfo.ADRootDSE.ServerType == ADServerType.ADDS && string.IsNullOrEmpty((string)this._cmdletParameters["SamAccountName"]))
			{
				this._cmdletParameters["SamAccountName"] = this._cmdletParameters["Name"];
			}
		}
	}
}