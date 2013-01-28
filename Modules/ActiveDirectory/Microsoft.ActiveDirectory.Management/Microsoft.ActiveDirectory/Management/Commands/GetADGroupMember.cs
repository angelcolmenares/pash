using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using System;
using System.Globalization;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Get", "ADGroupMember", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219308")]
	public class GetADGroupMember : ADCmdletBase<GetADGroupMemberParameterSet>, IADErrorTarget
	{
		private const string _debugCategory = "GetADGroupMember";

		private ADGroup _identityADGroup;

		private bool _isRecursive;

		private string _partitionPath;

		public GetADGroupMember()
		{
			base.BeginProcessPipeline.InsertAtStart(new CmdletSubroutine(base.GetADCmdletBaseExternalDelegates().AddSessionOptionWritableDCRequiredCSRoutine));
			base.ProcessRecordPipeline.InsertAtEnd(new CmdletSubroutine(this.GetADGroupMemberProcessCSRoutine));
		}

		private bool GetADGroupMemberProcessCSRoutine()
		{
			this._partitionPath = this._cmdletParameters["Partition"] as string;
			this._identityADGroup = this._cmdletParameters["Identity"] as ADGroup;
			this._isRecursive = this._cmdletParameters.GetSwitchParameterBooleanValue("Recursive");
			base.SetPipelinedSessionInfo(this._identityADGroup.SessionInfo);
			CmdletSessionInfo cmdletSessionInfo = base.GetCmdletSessionInfo();
			ADGroupFactory<ADGroup> aDGroupFactory = new ADGroupFactory<ADGroup>();
			aDGroupFactory.SetCmdletSessionInfo(cmdletSessionInfo);
			this.ValidateParameters();
			ADObject directoryObjectFromIdentity = aDGroupFactory.GetDirectoryObjectFromIdentity(this._identityADGroup, cmdletSessionInfo.DefaultPartitionPath);
			using (ADAccountManagement aDAccountManagement = new ADAccountManagement(cmdletSessionInfo.ADSessionInfo))
			{
				ADPrincipal[] groupMembers = aDAccountManagement.GetGroupMembers(cmdletSessionInfo.DefaultPartitionPath, directoryObjectFromIdentity.DistinguishedName, this._isRecursive);
				ADPrincipal[] aDPrincipalArray = groupMembers;
				for (int i = 0; i < (int)aDPrincipalArray.Length; i++)
				{
					ADPrincipal aDPrincipal = aDPrincipalArray[i];
					base.WriteObject(aDPrincipal);
				}
			}
			return true;
		}

		object Microsoft.ActiveDirectory.Management.Commands.IADErrorTarget.CurrentIdentity(Exception e)
		{
			return this._identityADGroup;
		}

		protected internal virtual void ValidateParameters()
		{
			this.GetCmdletSessionInfo();
			if (!string.IsNullOrEmpty(this.GetDefaultPartitionPath()))
			{
				return;
			}
			else
			{
				object[] objArray = new object[1];
				objArray[0] = "Partition";
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.ParameterRequired, objArray));
			}
		}
	}
}