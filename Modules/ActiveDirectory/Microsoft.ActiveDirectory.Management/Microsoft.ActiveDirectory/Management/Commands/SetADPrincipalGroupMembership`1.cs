using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class SetADPrincipalGroupMembership<P> : ADSetCmdletBase<P, ADPrincipalFactory<ADPrincipal>, ADPrincipal>
	where P : ADParameterSet, new()
	{
		private const string _debugCategory = "SetADPrincipalGroupMembership";

		private bool _isMemberOfValidated;

		private SetADPrincipalGroupMembershipOperationType _operationType;

		private StringBuilder _command;

		private string _perGroupOperationFailedMessage;

		private string _overallOperationFailedMessage;

		private List<SetADPrincipalGroupMembership<P>.ADGroupPartitionDNPair> _validExtendedGroupPartitionPairList;

		private List<ADPrincipal> _validExtendedPrincipalList;

		static SetADPrincipalGroupMembership()
		{
		}

		internal SetADPrincipalGroupMembership(SetADPrincipalGroupMembershipOperationType operationType)
		{
			this._validExtendedPrincipalList = new List<ADPrincipal>();
			this._operationType = operationType;
			this.Init();
			base.BeginProcessPipeline.Clear();
			base.ProcessRecordPipeline.Clear();
			base.ProcessRecordPipeline.InsertAtEnd(new CmdletSubroutine(this.SetADPrincipalGroupMembershipProcessCSRoutine));
			base.EndProcessPipeline.InsertAtStart(new CmdletSubroutine(this.SetADPrincipalGroupMembershipEndCSRoutine));
		}

		private void Init()
		{
			if (this._operationType != SetADPrincipalGroupMembershipOperationType.AddGroupMembership)
			{
				if (this._operationType == SetADPrincipalGroupMembershipOperationType.RemoveGroupMembership)
				{
					this._command = new StringBuilder("Remove-ADGroupMember ");
					this._perGroupOperationFailedMessage = StringResources.FailedRemovingMembersFromGroup;
					this._overallOperationFailedMessage = StringResources.FailedRemovingMembersFromOneOrMoreGroup;
				}
			}
			else
			{
				this._command = new StringBuilder("Add-ADGroupMember ");
				this._perGroupOperationFailedMessage = StringResources.FailedAddingMembersToGroup;
				this._overallOperationFailedMessage = StringResources.FailedAddingMembersToOneOrMoreGroup;
			}
			this._command.Append(" -identity $args[0] -members $args[1] -partition $args[2] -Confirm:$args[3] ");
		}

		private bool SetADPrincipalGroupMembershipEndCSRoutine()
		{
			string defaultPartitionPath;
			string str = null;
			string str1 = null;
			string str2 = null;
			SetADPrincipalGroupMembershipOperationType setADPrincipalGroupMembershipOperationType = this._operationType;
			switch (setADPrincipalGroupMembershipOperationType)
			{
				case SetADPrincipalGroupMembershipOperationType.AddGroupMembership:
				{
					str = string.Format(CultureInfo.CurrentCulture, StringResources.AddADPrincipalGroupMembershipShouldProcessDescription, new object[0]);
					str2 = string.Format(CultureInfo.CurrentCulture, StringResources.AddADPrincipalGroupMembershipShouldProcessCaption, new object[0]);
					str1 = string.Format(CultureInfo.CurrentCulture, StringResources.AddADPrincipalGroupMembershipShouldProcessWarning, new object[0]);
					break;
				}
				case SetADPrincipalGroupMembershipOperationType.RemoveGroupMembership:
				{
					str = string.Format(CultureInfo.CurrentCulture, StringResources.RemoveADPrincipalGroupMembershipShouldProcessDescription, new object[0]);
					str2 = string.Format(CultureInfo.CurrentCulture, StringResources.RemoveADPrincipalGroupMembershipShouldProcessCaption, new object[0]);
					str1 = string.Format(CultureInfo.CurrentCulture, StringResources.RemoveADPrincipalGroupMembershipShouldProcessWarning, new object[0]);
					break;
				}
			}
			if (base.ShouldProcessOverride(str, str1, str2))
			{
				ADPrincipal[] array = this._validExtendedPrincipalList.ToArray();
				List<ADGroup> aDGroups = new List<ADGroup>();
				foreach (SetADPrincipalGroupMembership<P>.ADGroupPartitionDNPair aDGroupPartitionDNPair in this._validExtendedGroupPartitionPairList)
				{
					if (aDGroupPartitionDNPair.PartitionDN == null)
					{
						defaultPartitionPath = this.GetDefaultPartitionPath();
					}
					else
					{
						defaultPartitionPath = aDGroupPartitionDNPair.PartitionDN;
					}
					string str3 = defaultPartitionPath;
					try
					{
						object[] group = new object[4];
						group[0] = aDGroupPartitionDNPair.Group;
						group[1] = array;
						group[2] = str3;
						group[3] = false;
						base.InvokeCommand.InvokeScript(this._command.ToString(), false, PipelineResultTypes.None, null, group);
					}
					catch (RuntimeException runtimeException1)
					{
						RuntimeException runtimeException = runtimeException1;
						object[] distinguishedName = new object[2];
						distinguishedName[0] = aDGroupPartitionDNPair.Group.DistinguishedName;
						distinguishedName[1] = runtimeException.Message;
						string str4 = string.Format(CultureInfo.CurrentCulture, this._perGroupOperationFailedMessage, distinguishedName);
						base.WriteWarning(str4);
						DebugLogger.LogError("SetADPrincipalGroupMembership", str4);
						aDGroups.Add(aDGroupPartitionDNPair.Group);
					}
				}
				if (aDGroups.Count <= 0)
				{
					if (this._cmdletParameters.GetSwitchParameterBooleanValue("PassThru"))
					{
						foreach (ADPrincipal aDPrincipal in this._validExtendedPrincipalList)
						{
							base.WriteObject(aDPrincipal);
						}
					}
				}
				else
				{
					ADException aDException = new ADException(this._overallOperationFailedMessage);
					base.ThrowTerminatingError(new ErrorRecord(aDException, "1", ErrorCategory.OperationStopped, aDGroups.ToArray()));
				}
				return true;
			}
			else
			{
				DebugLogger.LogInfo("SetADPrincipalGroupMembership", "User selected NO when asked to confirm..exiting without doing anything.");
				return false;
			}
		}

		private bool SetADPrincipalGroupMembershipProcessCSRoutine()
		{
			if (this._cmdletParameters.Contains("Identity"))
			{
				ADPrincipal item = this._cmdletParameters["Identity"] as ADPrincipal;
				this.SetPipelinedSessionInfo(item.SessionInfo);
				CmdletSessionInfo cmdletSessionInfo = base.GetCmdletSessionInfo();
				this._factory.SetCmdletSessionInfo(cmdletSessionInfo);
				base.ValidateParameters();
				this.ValidateMemberOfParameter();
				try
				{
					ADPrincipal extendedObjectFromIdentity = this._factory.GetExtendedObjectFromIdentity(item, cmdletSessionInfo.DefaultPartitionPath);
					this._validExtendedPrincipalList.Add(extendedObjectFromIdentity);
				}
				catch (ADIdentityNotFoundException aDIdentityNotFoundException1)
				{
					ADIdentityNotFoundException aDIdentityNotFoundException = aDIdentityNotFoundException1;
					base.ThrowTerminatingError(ADUtilities.GetErrorRecord(aDIdentityNotFoundException, "SetADPrincipalGroupMembership:ProcessRecordOverride", item));
				}
				return true;
			}
			else
			{
				object[] objArray = new object[1];
				objArray[0] = "Identity";
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.ParameterRequired, objArray));
			}
		}

		private void ValidateMemberOfParameter()
		{
			ADGroup extendedObjectFromIdentity;
			if (!this._isMemberOfValidated)
			{
				this._validExtendedGroupPartitionPairList = new List<SetADPrincipalGroupMembership<P>.ADGroupPartitionDNPair>();
				object item = this._cmdletParameters["MemberOf"];
				ADPrincipal[] aDPrincipalArray = item as ADGroup[];
				if (aDPrincipalArray == null)
				{
					ADGroup aDGroup = item as ADGroup;
					if (aDGroup != null)
					{
						ADGroup[] aDGroupArray = new ADGroup[1];
						aDGroupArray[0] = aDGroup;
						aDPrincipalArray = aDGroupArray;
					}
				}
				if (aDPrincipalArray != null)
				{
					new Hashtable();
					ADGroupFactory<ADGroup> aDGroupFactory = new ADGroupFactory<ADGroup>();
					CmdletSessionInfo cmdletSessionInfo = base.GetCmdletSessionInfo();
					aDGroupFactory.SetCmdletSessionInfo(cmdletSessionInfo);
					ADPrincipal[] aDPrincipalArray1 = aDPrincipalArray;
					for (int i = 0; i < (int)aDPrincipalArray1.Length; i++)
					{
						ADGroup aDGroup1 = (ADGroup)aDPrincipalArray1[i];
						try
						{
							if (!aDGroup1.IsSearchResult)
							{
								extendedObjectFromIdentity = aDGroupFactory.GetExtendedObjectFromIdentity(aDGroup1, cmdletSessionInfo.DefaultPartitionPath);
								this._validExtendedGroupPartitionPairList.Add(new SetADPrincipalGroupMembership<P>.ADGroupPartitionDNPair(extendedObjectFromIdentity));
							}
							else
							{
								extendedObjectFromIdentity = aDGroup1;
								using (ADObjectSearcher aDObjectSearcher = new ADObjectSearcher(extendedObjectFromIdentity.SessionInfo))
								{
									ADRootDSE rootDSE = aDObjectSearcher.GetRootDSE();
									string str = ADForestPartitionInfo.ExtractAndValidatePartitionInfo(rootDSE, extendedObjectFromIdentity.DistinguishedName);
									this._validExtendedGroupPartitionPairList.Add(new SetADPrincipalGroupMembership<P>.ADGroupPartitionDNPair(extendedObjectFromIdentity, str));
								}
							}
						}
						catch (ADIdentityNotFoundException aDIdentityNotFoundException1)
						{
							ADIdentityNotFoundException aDIdentityNotFoundException = aDIdentityNotFoundException1;
							DebugLogger.LogError("SetADPrincipalGroupMembership", aDIdentityNotFoundException.ToString());
							base.ThrowTerminatingError(new ErrorRecord(aDIdentityNotFoundException, "SetADPrincipalGroupMembership:ValidateMemberOfParameter", ErrorCategory.ObjectNotFound, aDGroup1));
						}
					}
				}
				this._isMemberOfValidated = true;
				return;
			}
			else
			{
				return;
			}
		}

		private class ADGroupPartitionDNPair
		{
			private ADGroup _group;

			private string _partitionDN;

			public ADGroup Group
			{
				get
				{
					return this._group;
				}
			}

			public string PartitionDN
			{
				get
				{
					return this._partitionDN;
				}
			}

			public ADGroupPartitionDNPair(ADGroup group)
			{
				this._group = group;
				this._partitionDN = null;
			}

			public ADGroupPartitionDNPair(ADGroup group, string partitionDN)
			{
				this._group = group;
				this._partitionDN = partitionDN;
			}
		}
	}
}