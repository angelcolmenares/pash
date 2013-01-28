using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Management.Automation;
using System.Security.Principal;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class SetADGroupMember<P> : ADSetCmdletBase<P, ADGroupFactory<ADGroup>, ADGroup>
	where P : ADParameterSet, new()
	{
		private const string _debugCategory = "SetADGroupMember";

		private SetADGroupMemberOperationType _operationType;

		private bool _isMembersValidated;

		internal SetADGroupMember(SetADGroupMemberOperationType operationType)
		{
			this._operationType = operationType;
			base.ProcessRecordPipeline.InsertAtStart(new CmdletSubroutine(this.SetADGroupMemberProcessCSRoutine));
		}

		private bool SetADGroupMemberProcessCSRoutine()
		{
			if (this._cmdletParameters.Contains("Identity"))
			{
				ADGroup item = this._cmdletParameters["Identity"] as ADGroup;
				this.SetPipelinedSessionInfo(item.SessionInfo);
				CmdletSessionInfo cmdletSessionInfo = base.GetCmdletSessionInfo();
				this._factory.SetCmdletSessionInfo(cmdletSessionInfo);
				base.ValidateParameters();
				this.ValidateMembersParameter();
				return true;
			}
			else
			{
				object[] objArray = new object[1];
				objArray[0] = "Identity";
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.ParameterRequired, objArray));
			}
		}

		private void ValidateMembersParameter()
		{
			bool flag;
			if (!this._isMembersValidated)
			{
				object item = this._cmdletParameters["Members"];
				ADPrincipal[] aDPrincipalArray = item as ADPrincipal[];
				if (aDPrincipalArray == null)
				{
					ADPrincipal aDPrincipal = item as ADPrincipal;
					if (aDPrincipal != null)
					{
						ADPrincipal[] aDPrincipalArray1 = new ADPrincipal[1];
						aDPrincipalArray1[0] = aDPrincipal;
						aDPrincipalArray = aDPrincipalArray1;
					}
				}
				if (aDPrincipalArray != null)
				{
					List<string> strs = new List<string>();
					Hashtable hashtables = new Hashtable();
					ADPrincipalFactory<ADPrincipal> aDPrincipalFactory = new ADPrincipalFactory<ADPrincipal>();
					CmdletSessionInfo cmdletSessionInfo = base.GetCmdletSessionInfo();
					aDPrincipalFactory.SetCmdletSessionInfo(cmdletSessionInfo);
					if (cmdletSessionInfo.ConnectedADServerType != ADServerType.ADDS)
					{
						flag = false;
					}
					else
					{
						flag = this._operationType == SetADGroupMemberOperationType.RemoveGroupMember;
					}
					bool flag1 = flag;
					Dictionary<SecurityIdentifier, string> securityIdentifiers = new Dictionary<SecurityIdentifier, string>();
					IADOPathNode aDOPathNode = null;
					SecurityIdentifier value = null;
					if (flag1)
					{
						ADGroup aDGroup = (ADGroup)this._cmdletParameters["Identity"];
						if (!aDGroup.IsSearchResult)
						{
							ADObject directoryObjectFromIdentity = aDPrincipalFactory.GetDirectoryObjectFromIdentity(aDGroup, cmdletSessionInfo.DefaultPartitionPath);
							value = (SecurityIdentifier)directoryObjectFromIdentity["objectSid"].Value;
						}
						else
						{
							value = aDGroup.SID;
						}
					}
					ADPrincipal[] aDPrincipalArray2 = aDPrincipalArray;
					for (int i = 0; i < (int)aDPrincipalArray2.Length; i++)
					{
						ADPrincipal aDPrincipal1 = aDPrincipalArray2[i];
						SecurityIdentifier sID = null;
						string distinguishedName = null;
						try
						{
							if (!aDPrincipal1.IsSearchResult)
							{
								ADObject aDObject = aDPrincipalFactory.GetDirectoryObjectFromIdentity(aDPrincipal1, cmdletSessionInfo.DefaultPartitionPath);
								sID = (SecurityIdentifier)aDObject["objectSid"].Value;
								distinguishedName = (string)aDObject["distinguishedName"].Value;
							}
							else
							{
								sID = aDPrincipal1.SID;
								distinguishedName = aDPrincipal1.DistinguishedName;
							}
							if (distinguishedName != null)
							{
								if (sID == null)
								{
									object[] objArray = new object[2];
									objArray[0] = "objectSid";
									objArray[1] = distinguishedName;
									throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.AttributeNotFoundOnObject, objArray));
								}
							}
							else
							{
								object[] identifyingString = new object[2];
								identifyingString[0] = "distinguishedName";
								identifyingString[1] = aDPrincipal1.IdentifyingString;
								throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.AttributeNotFoundOnObject, identifyingString));
							}
						}
						catch (ADIdentityNotFoundException aDIdentityNotFoundException1)
						{
							ADIdentityNotFoundException aDIdentityNotFoundException = aDIdentityNotFoundException1;
							DebugLogger.LogError("SetADGroupMember", aDIdentityNotFoundException.ToString());
							base.ThrowTerminatingError(new ErrorRecord(aDIdentityNotFoundException, "SetADGroupMember.ValidateMembersParameter", ErrorCategory.ObjectNotFound, aDPrincipal1));
						}
						catch (ArgumentException argumentException1)
						{
							ArgumentException argumentException = argumentException1;
							DebugLogger.LogError("SetADGroupMember", argumentException.ToString());
							base.ThrowTerminatingError(new ErrorRecord(argumentException, "SetADGroupMember.ValidateMembersParameter", ErrorCategory.ReadError, aDPrincipal1));
						}
						if (!flag1 || value.IsEqualDomainSid(sID))
						{
							strs.Add(Utils.ConvertSIDToStringizedSid(sID));
						}
						else
						{
							IADOPathNode aDOPathNode1 = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectSid", sID);
							if (aDOPathNode != null)
							{
								IADOPathNode[] aDOPathNodeArray = new IADOPathNode[2];
								aDOPathNodeArray[0] = aDOPathNode;
								aDOPathNodeArray[1] = aDOPathNode1;
								aDOPathNode = ADOPathUtil.CreateOrClause(aDOPathNodeArray);
							}
							else
							{
								aDOPathNode = aDOPathNode1;
							}
							securityIdentifiers.Add(sID, distinguishedName);
						}
					}
					if (aDOPathNode != null)
					{
						using (ADObjectSearcher aDObjectSearcher = new ADObjectSearcher(this.GetSessionInfo()))
						{
							IADOPathNode aDOPathNode2 = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectClass", "foreignSecurityPrincipal");
							aDObjectSearcher.SearchRoot = this.GetRootDSE().DefaultNamingContext;
							IADOPathNode[] aDOPathNodeArray1 = new IADOPathNode[2];
							aDOPathNodeArray1[0] = aDOPathNode2;
							aDOPathNodeArray1[1] = aDOPathNode;
							aDObjectSearcher.Filter = ADOPathUtil.CreateAndClause(aDOPathNodeArray1);
							aDObjectSearcher.Properties.Add("objectSid");
							foreach (ADObject aDObject1 in aDObjectSearcher.FindAll())
							{
								SecurityIdentifier securityIdentifier = (SecurityIdentifier)aDObject1["objectSid"].Value;
								if (!securityIdentifiers.ContainsKey(securityIdentifier))
								{
									continue;
								}
								strs.Add(Utils.ConvertSIDToStringizedSid(securityIdentifier));
								securityIdentifiers.Remove(securityIdentifier);
							}
							foreach (string str in securityIdentifiers.Values)
							{
								strs.Add(str);
							}
						}
					}
					if (this._operationType != SetADGroupMemberOperationType.AddGroupMember)
					{
						if (this._operationType == SetADGroupMemberOperationType.RemoveGroupMember)
						{
							hashtables.Add(PropertyModifyOp.Remove.ToString(), strs.ToArray());
						}
					}
					else
					{
						hashtables.Add(PropertyModifyOp.Add.ToString(), strs.ToArray());
					}
					this._cmdletParameters.RemoveParameter("Members");
					this._cmdletParameters["Members"] = new ADMultivalueHashtableParameter<string>(hashtables);
					this._isMembersValidated = true;
				}
				return;
			}
			else
			{
				return;
			}
		}
	}
}