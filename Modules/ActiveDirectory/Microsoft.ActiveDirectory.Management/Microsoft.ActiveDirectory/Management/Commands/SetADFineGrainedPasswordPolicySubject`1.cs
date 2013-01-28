using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class SetADFineGrainedPasswordPolicySubject<P> : ADSetCmdletBase<P, ADFineGrainedPasswordPolicyFactory<ADFineGrainedPasswordPolicy>, ADFineGrainedPasswordPolicy>
	where P : ADParameterSet, new()
	{
		private const string _debugCategory = "SetADFineGrainedPasswordPolicySubject";

		private SetSubjectOperationType _operationType;

		private List<string> _appliesToDNList;

		internal SetADFineGrainedPasswordPolicySubject(SetSubjectOperationType operationType)
		{
			this._appliesToDNList = new List<string>();
			this._operationType = operationType;
			base.BeginProcessPipeline.Clear();
			base.BeginProcessPipeline.InsertAtEnd(new CmdletSubroutine(this.SetADFGPPSubjectBeginCSRoutine));
			base.ProcessRecordPipeline.Clear();
			base.ProcessRecordPipeline.InsertAtEnd(new CmdletSubroutine(this.SetADFGPPSubjectProcessCSRoutine));
			base.EndProcessPipeline.Clear();
			base.EndProcessPipeline.InsertAtEnd(new CmdletSubroutine(this.SetADFGPPSubjectEndProcessCSRoutine));
			base.EndProcessPipeline.InsertAtEnd(new CmdletSubroutine(this.ADSetCmdletBaseProcessCSRoutine));
		}

		private bool SetADFGPPSubjectBeginCSRoutine()
		{
			ADFineGrainedPasswordPolicy item = this._cmdletParameters["Identity"] as ADFineGrainedPasswordPolicy;
			if (item.IsSearchResult)
			{
				this.SetPipelinedSessionInfo(item.SessionInfo);
			}
			CmdletSessionInfo cmdletSessionInfo = this.GetCmdletSessionInfo();
			this._factory.SetCmdletSessionInfo(cmdletSessionInfo);
			base.ValidateParameters();
			return true;
		}

		private bool SetADFGPPSubjectEndProcessCSRoutine()
		{
			Hashtable hashtables = new Hashtable();
			ADFineGrainedPasswordPolicy item = this._cmdletParameters["Identity"] as ADFineGrainedPasswordPolicy;
			if (item.IsSearchResult)
			{
				this.SetPipelinedSessionInfo(item.SessionInfo);
			}
			CmdletSessionInfo cmdletSessionInfo = this.GetCmdletSessionInfo();
			this._factory.SetCmdletSessionInfo(cmdletSessionInfo);
			if (this._operationType != SetSubjectOperationType.AddSubject)
			{
				if (this._operationType == SetSubjectOperationType.RemoveSubject)
				{
					hashtables.Add(PropertyModifyOp.Remove.ToString(), this._appliesToDNList.ToArray());
				}
			}
			else
			{
				hashtables.Add(PropertyModifyOp.Add.ToString(), this._appliesToDNList.ToArray());
			}
			this._cmdletParameters["AppliesTo"] = new ADMultivalueHashtableParameter<string>(hashtables);
			return true;
		}

		private bool SetADFGPPSubjectProcessCSRoutine()
		{
			if (this._cmdletParameters.Contains("Subjects"))
			{
				this.ValidateSubjectsAndAddToList();
				return true;
			}
			else
			{
				object[] objArray = new object[1];
				objArray[0] = "Subjects";
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.ParameterRequired, objArray));
			}
		}

		private void ValidateSubjectsAndAddToList()
		{
			object item = this._cmdletParameters["Subjects"];
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
			ADPrincipalFactory<ADPrincipal> aDPrincipalFactory = new ADPrincipalFactory<ADPrincipal>();
			if (aDPrincipalArray[0].IsSearchResult)
			{
				this.SetPipelinedSessionInfo(aDPrincipalArray[0].SessionInfo);
			}
			CmdletSessionInfo cmdletSessionInfo = base.GetCmdletSessionInfo();
			aDPrincipalFactory.SetCmdletSessionInfo(cmdletSessionInfo);
			if (aDPrincipalArray != null)
			{
				new Hashtable();
				ADPrincipal[] aDPrincipalArray2 = aDPrincipalArray;
				for (int i = 0; i < (int)aDPrincipalArray2.Length; i++)
				{
					ADPrincipal aDPrincipal1 = aDPrincipalArray2[i];
					try
					{
						ADObject directoryObjectFromIdentity = aDPrincipalFactory.GetDirectoryObjectFromIdentity(aDPrincipal1, cmdletSessionInfo.DefaultPartitionPath);
						this._appliesToDNList.Add(directoryObjectFromIdentity.DistinguishedName);
					}
					catch (ADIdentityNotFoundException aDIdentityNotFoundException1)
					{
						ADIdentityNotFoundException aDIdentityNotFoundException = aDIdentityNotFoundException1;
						DebugLogger.LogError("SetADFineGrainedPasswordPolicySubject", aDIdentityNotFoundException.ToString());
						base.ThrowTerminatingError(ADUtilities.GetErrorRecord(aDIdentityNotFoundException, "SetADFineGrainedPasswordPolicySubject:ValidateSubjectsAndAddToList", aDPrincipal1));
					}
				}
			}
		}
	}
}