using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using System;
using System.Globalization;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("New", "ADComputer", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219325", SupportsShouldProcess=true)]
	public class NewADComputer : ADNewCmdletBase<NewADComputerParameterSet, ADComputerFactory<ADComputer>, ADComputer>
	{
		private const string _debugCategory = "NewADComputer";

		public NewADComputer()
		{
		}

		protected internal override string GetDefaultCreationPathBase()
		{
			return Utils.GetWellKnownGuidDN(this.GetSessionInfo(), this.GetDefaultPartitionPath(), WellKnownGuids.ComputersContainerGuid);
		}

		protected internal override void ValidateParameters()
		{
			base.ValidateParameters();
			CmdletSessionInfo cmdletSessionInfo = this.GetCmdletSessionInfo();
			if (cmdletSessionInfo.ADRootDSE.ServerType == ADServerType.ADDS && string.IsNullOrEmpty((string)this._cmdletParameters["SamAccountName"]))
			{
				this._cmdletParameters["SamAccountName"] = this._cmdletParameters["Name"];
			}
			bool? item = (bool?)this._cmdletParameters["ChangePasswordAtLogon"];
			if (item.HasValue)
			{
				bool? nullable = (bool?)this._cmdletParameters["PasswordNeverExpires"];
				if (nullable.HasValue)
				{
					bool? item1 = (bool?)this._cmdletParameters["ChangePasswordAtLogon"];
					if (item1.Value)
					{
						bool? nullable1 = (bool?)this._cmdletParameters["PasswordNeverExpires"];
						if (nullable1.Value)
						{
							object[] objArray = new object[1];
							objArray[0] = "PasswordNeverExpires";
							throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.AcctChangePwdNotWorksWhenPwdNotExpires, objArray));
						}
					}
				}
			}
		}
	}
}