using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using System;
using System.Globalization;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Move", "ADDirectoryServerOperationMasterRole", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219322", SupportsShouldProcess=true, ConfirmImpact=ConfirmImpact.High)]
	public class MoveADDirectoryServerOperationMasterRole : ADCmdletBase<MoveADDirectoryServerOperationMasterRoleParameterSet>, IADErrorTarget
	{
		private const string _debugCategory = "MoveADDirectoryServerOperationMasterRole";

		private ADDirectoryServer _identityDSObj;

		private ADOperationMasterRole[] _operationMasterRolesToTransfer;

		private bool _seize;

		public MoveADDirectoryServerOperationMasterRole()
		{
			base.ProcessRecordPipeline.InsertAtEnd(new CmdletSubroutine(this.MoveADDSOperationMasterRoleProcessCSRoutine));
		}

		object Microsoft.ActiveDirectory.Management.Commands.IADErrorTarget.CurrentIdentity(Exception e)
		{
			return this._identityDSObj;
		}

		private bool MoveADDSOperationMasterRoleProcessCSRoutine()
		{
			ADObject aDObject;
			bool flag = false;
			string value;
			int? nullable;
			this._identityDSObj = this._cmdletParameters["Identity"] as ADDirectoryServer;
			this._seize = this._cmdletParameters.GetSwitchParameterBooleanValue("Force");
			this._operationMasterRolesToTransfer = this._cmdletParameters["OperationMasterRole"] as ADOperationMasterRole[];
			base.SetPipelinedSessionInfo(this._identityDSObj.SessionInfo);
			CmdletSessionInfo cmdletSessionInfo = this.GetCmdletSessionInfo();
			if (cmdletSessionInfo.ConnectedADServerType == ADServerType.ADLDS)
			{
				ADOperationMasterRole[] aDOperationMasterRoleArray = this._operationMasterRolesToTransfer;
				for (int i = 0; i < (int)aDOperationMasterRoleArray.Length; i++)
				{
					ADOperationMasterRole aDOperationMasterRole = aDOperationMasterRoleArray[i];
					ADOperationMasterRole aDOperationMasterRole1 = aDOperationMasterRole;
					switch (aDOperationMasterRole1)
					{
						case ADOperationMasterRole.PDCEmulator:
						case ADOperationMasterRole.RIDMaster:
						case ADOperationMasterRole.InfrastructureMaster:
						{
							object[] str = new object[1];
							str[0] = aDOperationMasterRole.ToString();
							throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.MoveOperationMasterRoleNotApplicableForADLDS, str));
						}
					}
				}
			}
			ADDirectoryServerFactory<ADDirectoryServer> aDDirectoryServerFactory = new ADDirectoryServerFactory<ADDirectoryServer>();
			aDDirectoryServerFactory.SetCmdletSessionInfo(cmdletSessionInfo);
			ADObject directoryObjectFromIdentity = aDDirectoryServerFactory.GetDirectoryObjectFromIdentity(this._identityDSObj, cmdletSessionInfo.DefaultPartitionPath);
			string distinguishedName = directoryObjectFromIdentity.DistinguishedName;
			ADObjectSearcher aDObjectSearcher = SearchUtility.BuildSearcher(cmdletSessionInfo.ADSessionInfo, distinguishedName, ADSearchScope.Base);
			using (aDObjectSearcher)
			{
				aDObjectSearcher.Filter = ADOPathUtil.CreateFilterClause(ADOperator.Like, "objectClass", "*");
				aDObjectSearcher.Properties.Add("dNSHostName");
				directoryObjectFromIdentity = aDObjectSearcher.FindOne();
				if (directoryObjectFromIdentity != null)
				{
					if (directoryObjectFromIdentity["dNSHostName"] == null)
					{
						value = null;
					}
					else
					{
						value = (string)directoryObjectFromIdentity["dNSHostName"].Value;
					}
					string str1 = value;
					if (!string.IsNullOrEmpty(str1))
					{
						if (cmdletSessionInfo.ConnectedADServerType == ADServerType.ADLDS)
						{
							string str2 = string.Concat("CN=NTDS Settings,", distinguishedName);
							ADObjectSearcher aDObjectSearcher1 = SearchUtility.BuildSearcher(cmdletSessionInfo.ADSessionInfo, str2, ADSearchScope.Base);
							using (aDObjectSearcher1)
							{
								aDObjectSearcher1.Filter = ADOPathUtil.CreateFilterClause(ADOperator.Like, "objectClass", "*");
								aDObjectSearcher1.Properties.Add("msDS-PortLDAP");
								aDObject = aDObjectSearcher1.FindOne();
							}
							if (aDObject != null)
							{
								if (aDObject["msDS-PortLDAP"] == null)
								{
									int? nullable1 = null;
									nullable = nullable1;
								}
								else
								{
									nullable = (int?)aDObject["msDS-PortLDAP"].Value;
								}
								int? nullable2 = nullable;
								if (nullable2.HasValue)
								{
									str1 = string.Concat(str1, ":", nullable2);
								}
								else
								{
									object[] objArray = new object[2];
									objArray[0] = "msDS-PortLDAP";
									objArray[1] = aDObject.DistinguishedName;
									throw new ADException(string.Format(CultureInfo.CurrentCulture, StringResources.AttributeNotFoundOnObject, objArray));
								}
							}
							else
							{
								throw new ADIdentityNotFoundException(string.Concat(StringResources.ObjectNotFound, " : ", str2));
							}
						}
						ADSessionInfo aDSessionInfo = cmdletSessionInfo.ADSessionInfo.Copy();
						aDSessionInfo.Server = str1;
						using (ADTopologyManagement aDTopologyManagement = new ADTopologyManagement(aDSessionInfo))
						{
							ADOperationMasterRole[] aDOperationMasterRoleArray1 = this._operationMasterRolesToTransfer;
							for (int j = 0; j < (int)aDOperationMasterRoleArray1.Length; j++)
							{
								ADOperationMasterRole aDOperationMasterRole2 = aDOperationMasterRoleArray1[j];
								string str3 = string.Format(CultureInfo.CurrentCulture, StringResources.MoveOperationMasterRoleDescription, new object[0]);
								object[] objArray1 = new object[2];
								objArray1[0] = aDOperationMasterRole2.ToString();
								objArray1[1] = str1;
								string str4 = string.Format(CultureInfo.CurrentCulture, StringResources.MoveOperationMasterRoleWarning, objArray1);
								string str5 = string.Format(CultureInfo.CurrentCulture, StringResources.MoveOperationMasterRoleCaption, new object[0]);
								if (base.ShouldProcessOverride(str3, str4, str5))
								{
									aDTopologyManagement.MoveOperationMasterRole(aDOperationMasterRole2, this._seize, out flag);
								}
							}
						}
					}
					else
					{
						object[] distinguishedName1 = new object[2];
						distinguishedName1[0] = "dNSHostName";
						distinguishedName1[1] = directoryObjectFromIdentity.DistinguishedName;
						throw new ADException(string.Format(CultureInfo.CurrentCulture, StringResources.AttributeNotFoundOnObject, distinguishedName1));
					}
				}
				else
				{
					throw new ADIdentityNotFoundException(string.Concat(StringResources.ObjectNotFound, " : ", distinguishedName));
				}
			}
			return true;
		}
	}
}