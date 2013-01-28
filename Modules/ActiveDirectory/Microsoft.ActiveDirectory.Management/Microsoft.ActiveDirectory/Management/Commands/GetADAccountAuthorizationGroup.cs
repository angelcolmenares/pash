using Microsoft.ActiveDirectory.Management;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Get", "ADAccountAuthorizationGroup", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219317", DefaultParameterSetName="Identity")]
	public class GetADAccountAuthorizationGroup : ADCmdletBase<GetADAccountAuthorizationGroupParameterSet>, IADErrorTarget
	{
		private const string _debugCategory = "GetADAccountAuthorizationGroup";

		private ADAccount _identityADAccount;

		private string _partitionPath;

		public GetADAccountAuthorizationGroup()
		{
			base.BeginProcessPipeline.InsertAtStart(new CmdletSubroutine(base.GetADCmdletBaseExternalDelegates().AddSessionOptionWritableDCRequiredCSRoutine));
			base.ProcessRecordPipeline.InsertAtEnd(new CmdletSubroutine(this.GetADAccountAuthorizationGroupProcessCSRoutine));
		}

		private bool GetADAccountAuthorizationGroupProcessCSRoutine()
		{
			this._partitionPath = this._cmdletParameters["Partition"] as string;
			this._identityADAccount = this._cmdletParameters["Identity"] as ADAccount;
			base.SetPipelinedSessionInfo(this._identityADAccount.SessionInfo);
			CmdletSessionInfo cmdletSessionInfo = base.GetCmdletSessionInfo();
			ADAccountFactory<ADAccount> aDAccountFactory = new ADAccountFactory<ADAccount>();
			aDAccountFactory.SetCmdletSessionInfo(cmdletSessionInfo);
			ADObject directoryObjectFromIdentity = aDAccountFactory.GetDirectoryObjectFromIdentity(this._identityADAccount, cmdletSessionInfo.DefaultPartitionPath);
			using (ADAccountManagement aDAccountManagement = new ADAccountManagement(cmdletSessionInfo.ADSessionInfo))
			{
				ADGroup[] authorizationGroups = aDAccountManagement.GetAuthorizationGroups(cmdletSessionInfo.DefaultPartitionPath, directoryObjectFromIdentity.DistinguishedName);
				ADGroup[] aDGroupArray = authorizationGroups;
				for (int i = 0; i < (int)aDGroupArray.Length; i++)
				{
					ADGroup aDGroup = aDGroupArray[i];
					base.WriteObject(aDGroup);
				}
			}
			return true;
		}

		object Microsoft.ActiveDirectory.Management.Commands.IADErrorTarget.CurrentIdentity(Exception e)
		{
			return this._identityADAccount;
		}
	}
}