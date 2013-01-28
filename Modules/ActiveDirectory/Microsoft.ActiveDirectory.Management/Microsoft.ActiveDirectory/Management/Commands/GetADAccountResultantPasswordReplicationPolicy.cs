using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using System;
using System.Globalization;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Get", "ADAccountResultantPasswordReplicationPolicy", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219312")]
	public class GetADAccountResultantPasswordReplicationPolicy : ADCmdletBase<GetADAccountResultantPasswordReplicationPolicyParameterSet>, IADErrorTarget
	{
		private const string _debugCategory = "GetADAccountResultantPasswordReplicationPolicy";

		private ADAccount _identityADAccount;

		private ADDomainController _targetDCAccount;

		private string _partitionPath;

		public GetADAccountResultantPasswordReplicationPolicy()
		{
			base.BeginProcessPipeline.InsertAtStart(new CmdletSubroutine(base.GetADCmdletBaseExternalDelegates().AddSessionOptionWindows2008AndAboveRequiredCSRoutine));
			base.BeginProcessPipeline.InsertAtStart(new CmdletSubroutine(base.GetADCmdletBaseExternalDelegates().AddSessionOptionWritableDCRequiredCSRoutine));
			base.ProcessRecordPipeline.Clear();
			base.ProcessRecordPipeline.InsertAtEnd(new CmdletSubroutine(this.GetADAcctResPrpProcessCSRoutine));
		}

		private bool GetADAcctResPrpProcessCSRoutine()
		{
			ADResultantPasswordReplicationPolicy aDResultantPasswordReplicationPolicy = ADResultantPasswordReplicationPolicy.DenyExplicit;
			int? value;
			this._partitionPath = this._cmdletParameters["Partition"] as string;
			this._identityADAccount = this._cmdletParameters["Identity"] as ADAccount;
			this._targetDCAccount = this._cmdletParameters["DomainController"] as ADDomainController;
			base.SetPipelinedSessionInfo(this._identityADAccount.SessionInfo);
			CmdletSessionInfo cmdletSessionInfo = base.GetCmdletSessionInfo();
			if (cmdletSessionInfo.ADRootDSE.IsWindows2008AndAbove())
			{
				if (!cmdletSessionInfo.ADRootDSE.IsWritable())
				{
					object[] dNSHostName = new object[1];
					dNSHostName[0] = cmdletSessionInfo.ADRootDSE.DNSHostName;
					base.WriteWarning(string.Format(CultureInfo.CurrentCulture, StringResources.WarningResultantPRPNotAccurateOnRODC, dNSHostName));
				}
				ADDomainControllerFactory<ADDomainController> aDDomainControllerFactory = new ADDomainControllerFactory<ADDomainController>();
				aDDomainControllerFactory.SetCmdletSessionInfo(cmdletSessionInfo);
				ADAccountFactory<ADAccount> aDAccountFactory = new ADAccountFactory<ADAccount>();
				aDAccountFactory.SetCmdletSessionInfo(cmdletSessionInfo);
				ADObject directoryObjectFromIdentity = aDDomainControllerFactory.GetDirectoryObjectFromIdentity(this._targetDCAccount, cmdletSessionInfo.DefaultPartitionPath);
				ADObject aDObject = aDAccountFactory.GetDirectoryObjectFromIdentity(this._identityADAccount, cmdletSessionInfo.DefaultPartitionPath);
				ADObjectSearcher distinguishedName = SearchUtility.BuildSearcher(cmdletSessionInfo.ADSessionInfo, directoryObjectFromIdentity.DistinguishedName, ADSearchScope.Base);
				using (distinguishedName)
				{
					distinguishedName.Filter = ADOPathUtil.CreateFilterClause(ADOperator.Like, "objectClass", "*");
					distinguishedName.Properties.Add("msDS-IsUserCachableAtRodc");
					distinguishedName.InputDN = aDObject.DistinguishedName;
					ADObject aDObject1 = distinguishedName.FindOne();
					if (aDObject1["msDS-IsUserCachableAtRodc"] == null)
					{
						int? nullable = null;
						value = nullable;
					}
					else
					{
						value = (int?)aDObject1["msDS-IsUserCachableAtRodc"].Value;
					}
					int? nullable1 = value;
					if (nullable1.HasValue)
					{
						if (!Utils.TryParseEnum<ADResultantPasswordReplicationPolicy>(nullable1.ToString(), out aDResultantPasswordReplicationPolicy))
						{
							DebugLogger.LogInfo("GetADAccountResultantPasswordReplicationPolicy", string.Format("Error parsing resultant prp: {0} for account {1} on RODC {2}", aDResultantPasswordReplicationPolicy, aDObject.DistinguishedName, directoryObjectFromIdentity.DistinguishedName));
							base.WriteObject(ADResultantPasswordReplicationPolicy.Unknown);
						}
						else
						{
							base.WriteObject(aDResultantPasswordReplicationPolicy);
						}
					}
					else
					{
						base.WriteObject(ADResultantPasswordReplicationPolicy.Unknown);
					}
				}
				return true;
			}
			else
			{
				base.WriteError(new ErrorRecord(new ArgumentException(StringResources.ErrorResultantPRPSpecifyWindows2008OrAbove), "GetADAccountResultantPasswordReplicationPolicy:ProcessRecord", ErrorCategory.InvalidData, null));
				return false;
			}
		}

		object Microsoft.ActiveDirectory.Management.Commands.IADErrorTarget.CurrentIdentity(Exception e)
		{
			return this._identityADAccount;
		}
	}
}