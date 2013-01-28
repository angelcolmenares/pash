using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security.Principal;
using System.Text;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class SetADDCPasswordReplicationPolicy<P> : ADCmdletBase<P>, IADErrorTarget
	where P : ADParameterSet, new()
	{
		private const string _debugCategory = "SetADDCPasswordReplicationPolicy";

		private SetADDCPasswordReplicationPolicyOperationType _operationType;

		private ADDomainController _identityDCObj;

		private Hashtable _principalsToAddOrRemove;

		internal SetADDCPasswordReplicationPolicy(SetADDCPasswordReplicationPolicyOperationType operationType)
		{
			this._operationType = operationType;
			base.BeginProcessPipeline.InsertAtEnd(new CmdletSubroutine(this.SetADDCPrpBeginCSRoutine));
			base.ProcessRecordPipeline.InsertAtEnd(new CmdletSubroutine(this.SetADDCPrpProcessCSRoutine));
		}

		object Microsoft.ActiveDirectory.Management.Commands.IADErrorTarget.CurrentIdentity(Exception e)
		{
			return this._identityDCObj;
		}

		private bool SetADDCPrpBeginCSRoutine()
		{
			SecurityIdentifier value;
			ADPrincipal[] item = null;
			if (base.ParameterSetName != "AllowedPRP")
			{
				if (base.ParameterSetName == "DeniedPRP")
				{
					item = this._cmdletParameters["DeniedList"] as ADPrincipal[];
				}
			}
			else
			{
				item = this._cmdletParameters["AllowedList"] as ADPrincipal[];
			}
			if (item != null)
			{
				List<string> strs = new List<string>();
				ADPrincipalFactory<ADPrincipal> aDPrincipalFactory = new ADPrincipalFactory<ADPrincipal>();
				ADPrincipal[] aDPrincipalArray = item;
				for (int i = 0; i < (int)aDPrincipalArray.Length; i++)
				{
					ADPrincipal aDPrincipal = aDPrincipalArray[i];
					base.SetPipelinedSessionInfo(aDPrincipal.SessionInfo);
					CmdletSessionInfo cmdletSessionInfo = base.GetCmdletSessionInfo();
					aDPrincipalFactory.SetCmdletSessionInfo(cmdletSessionInfo);
					try
					{
						if (!aDPrincipal.IsSearchResult)
						{
							ADObject directoryObjectFromIdentity = aDPrincipalFactory.GetDirectoryObjectFromIdentity(aDPrincipal, cmdletSessionInfo.DefaultPartitionPath);
							value = (SecurityIdentifier)directoryObjectFromIdentity["objectSid"].Value;
						}
						else
						{
							value = aDPrincipal.SID;
						}
						if (value != null)
						{
							string stringizedSid = Utils.ConvertSIDToStringizedSid(value);
							strs.Add(stringizedSid);
						}
						else
						{
							object[] distinguishedName = new object[2];
							distinguishedName[0] = "objectSid";
							distinguishedName[1] = aDPrincipal.DistinguishedName;
							throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.AttributeNotFoundOnObject, distinguishedName));
						}
					}
					catch (ADIdentityNotFoundException aDIdentityNotFoundException1)
					{
						ADIdentityNotFoundException aDIdentityNotFoundException = aDIdentityNotFoundException1;
						DebugLogger.LogError("SetADDCPasswordReplicationPolicy", aDIdentityNotFoundException.ToString());
						base.ThrowTerminatingError(new ErrorRecord(aDIdentityNotFoundException, "SetADDCPasswordReplicationPolicy:BeginProcessing", ErrorCategory.ReadError, aDPrincipal));
					}
					catch (ArgumentException argumentException1)
					{
						ArgumentException argumentException = argumentException1;
						DebugLogger.LogError("SetADDCPasswordReplicationPolicy", argumentException.ToString());
						base.ThrowTerminatingError(new ErrorRecord(argumentException, "SetADDCPasswordReplicationPolicy:BeginProcessing", ErrorCategory.ReadError, aDPrincipal));
					}
				}
				this._principalsToAddOrRemove = new Hashtable();
				if (base.ParameterSetName != "AllowedPRP")
				{
					if (base.ParameterSetName == "DeniedPRP")
					{
						this._principalsToAddOrRemove.Add("msDS-NeverRevealGroup", strs.ToArray());
					}
				}
				else
				{
					this._principalsToAddOrRemove.Add("msDS-RevealOnDemandGroup", strs.ToArray());
				}
			}
			return true;
		}

		private bool SetADDCPrpProcessCSRoutine()
		{
			this._identityDCObj = this._cmdletParameters["Identity"] as ADDomainController;
			this.SetPipelinedSessionInfo(this._identityDCObj.SessionInfo);
			CmdletSessionInfo cmdletSessionInfo = this.GetCmdletSessionInfo();
			ADDomainControllerFactory<ADDomainController> aDDomainControllerFactory = new ADDomainControllerFactory<ADDomainController>();
			aDDomainControllerFactory.SetCmdletSessionInfo(cmdletSessionInfo);
			ADObject directoryObjectFromIdentity = aDDomainControllerFactory.GetDirectoryObjectFromIdentity(this._identityDCObj, cmdletSessionInfo.DefaultPartitionPath);
			StringBuilder stringBuilder = new StringBuilder("Set-ADObject -identity $args[0] ");
			if (this._operationType != SetADDCPasswordReplicationPolicyOperationType.AddPasswordReplicationPolicy)
			{
				if (this._operationType == SetADDCPasswordReplicationPolicyOperationType.RemovePasswordReplicationPolicy)
				{
					stringBuilder.Append(" -Remove ");
				}
			}
			else
			{
				stringBuilder.Append(" -Add ");
			}
			stringBuilder.Append(" $args[1] ");
			if (base.ShouldProcessOverride(directoryObjectFromIdentity.IdentifyingString))
			{
				try
				{
					object[] objArray = new object[2];
					objArray[0] = directoryObjectFromIdentity;
					objArray[1] = this._principalsToAddOrRemove;
					base.InvokeCommand.InvokeScript(stringBuilder.ToString(), false, PipelineResultTypes.Output, null, objArray);
				}
				catch (RuntimeException runtimeException1)
				{
					RuntimeException runtimeException = runtimeException1;
					object[] message = new object[1];
					message[0] = runtimeException.Message;
					string str = string.Format(CultureInfo.CurrentCulture, "Failed adding or removing the password-replication-policy:  {0}", message);
					DebugLogger.LogError("SetADDCPasswordReplicationPolicy", str);
					base.WriteError(new ErrorRecord(runtimeException, "0", ErrorCategory.WriteError, this._identityDCObj));
				}
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}