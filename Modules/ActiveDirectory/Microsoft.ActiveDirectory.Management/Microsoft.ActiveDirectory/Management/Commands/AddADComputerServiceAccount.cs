using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Add", "ADComputerServiceAccount", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219291", SupportsShouldProcess=true)]
	public class AddADComputerServiceAccount : SetADComputerServiceAccount<AddADComputerServiceAccountParameterSet>
	{
		public AddADComputerServiceAccount() : base(((PropertyModifyOp)0).ToString())
		{
			base.ProcessRecordPipeline.InsertAtStart(new CmdletSubroutine(this.AddADComputerServiceAccountProcessCSRoutine));
		}

		private bool AddADComputerServiceAccountProcessCSRoutine()
		{
			CmdletSessionInfo cmdletSessionInfo = this.GetCmdletSessionInfo();
			ADComputerFactory<ADComputer> aDComputerFactory = new ADComputerFactory<ADComputer>();
			aDComputerFactory.SetCmdletSessionInfo(cmdletSessionInfo);
			ADObject directoryObjectFromIdentity = aDComputerFactory.GetDirectoryObjectFromIdentity(this._cmdletParameters["Identity"] as ADComputer, cmdletSessionInfo.DefaultPartitionPath);
			string distinguishedName = directoryObjectFromIdentity.DistinguishedName;
			for (int i = 0; i < this._resolvedServiceAccountList.Count; i++)
			{
				object value = this._resolvedServiceAccountList[i]["msDS-HostServiceAccountBL"].Value;
				string[] strArrays = value as string[];
				if (strArrays == null)
				{
					string str = (string)this._resolvedServiceAccountList[i]["msDS-HostServiceAccountBL"].Value;
					if (str != null)
					{
						string[] strArrays1 = new string[1];
						strArrays1[0] = str;
						strArrays = strArrays1;
					}
				}
				if (strArrays != null)
				{
					int num = 0;
					while (num < (int)strArrays.Length)
					{
						if (strArrays[num].Equals(distinguishedName) || this._cmdletParameters.GetSwitchParameterBooleanValue("Force") || base.ShouldContinue(string.Format(StringResources.OtherBackLinkDescription, this._resolvedServiceAccountList[i].DistinguishedName, strArrays[num]), StringResources.OtherBackLinkCaption))
						{
							num++;
						}
						else
						{
							return false;
						}
					}
				}
			}
			return true;
		}
	}
}