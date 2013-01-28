using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public abstract class ADSetObjectMember<P, F, O, MF, MO> : ADSetCmdletBase<P, F, O>
	where P : ADParameterSet, new()
	where F : ADFactory<O>, new()
	where O : ADEntity, new()
	where MF : ADFactory<MO>, new()
	where MO : ADEntity, new()
	{
		private SetADMemberOperationType _operationType;

		private bool _isMembersValidated;

		internal ADSetObjectMember(SetADMemberOperationType operationType)
		{
			this._operationType = operationType;
			base.ProcessRecordPipeline.InsertAtStart(new CmdletSubroutine(this.SetADObjectMemberProcessCSRoutine));
		}

		internal abstract string GetMemberDefaultPartitionPath();

		protected virtual void ProcessMembersParameter()
		{
			string value;
			if (!this._isMembersValidated)
			{
				string memberDefaultPartitionPath = this.GetMemberDefaultPartitionPath();
				object item = this._cmdletParameters["Members"];
				MO[] mOArray = item as MO[];
				if (mOArray != null)
				{
					List<string> strs = new List<string>();
					Hashtable hashtables = new Hashtable();
					MF mF = Activator.CreateInstance<MF>();
					CmdletSessionInfo cmdletSessionInfo = base.GetCmdletSessionInfo();
					mF.SetCmdletSessionInfo(cmdletSessionInfo);
					MO[] mOArray1 = mOArray;
					int num = 0;
					while (num < (int)mOArray1.Length)
					{
						MO mO = mOArray1[num];
						if (!mO.IsSearchResult)
						{
							ADObject directoryObjectFromIdentity = mF.GetDirectoryObjectFromIdentity(mO, memberDefaultPartitionPath);
							value = directoryObjectFromIdentity["distinguishedName"].Value as string;
						}
						else
						{
							value = mO["distinguishedName"].Value as string;
						}
						if (value != null)
						{
							strs.Add(value);
							num++;
						}
						else
						{
							object[] identifyingString = new object[2];
							identifyingString[0] = "distinguishedName";
							identifyingString[1] = mO.IdentifyingString;
							throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.AttributeNotFoundOnObject, identifyingString));
						}
					}
					if (this._operationType != SetADMemberOperationType.AddMember)
					{
						if (this._operationType == SetADMemberOperationType.RemoveMember)
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

		private bool SetADObjectMemberProcessCSRoutine()
		{
			this.ProcessMembersParameter();
			return true;
		}
	}
}