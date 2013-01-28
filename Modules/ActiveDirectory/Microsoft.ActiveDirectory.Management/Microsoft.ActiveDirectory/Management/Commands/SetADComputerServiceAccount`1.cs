using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class SetADComputerServiceAccount<P> : ADSetCmdletBase<P, ADComputerFactory<ADComputer>, ADComputer>
	where P : ADParameterSet, new()
	{
		private const string _debugCategory = "SetADComputerServiceAccount";

		private string _modifyOpString;

		protected List<ADObject> _resolvedServiceAccountList;

		public SetADComputerServiceAccount(string modifyOpString)
		{
			this._resolvedServiceAccountList = new List<ADObject>();
			this._modifyOpString = modifyOpString;
			base.BeginProcessPipeline.InsertAtStart(new CmdletSubroutine(this.SetADComputerServiceAccountBeginCSRoutine));
		}

		private bool SetADComputerServiceAccountBeginCSRoutine()
		{
			object item = this._cmdletParameters["ServiceAccount"];
			ADServiceAccount[] aDServiceAccountArray = item as ADServiceAccount[];
			if (aDServiceAccountArray == null)
			{
				ADServiceAccount aDServiceAccount = item as ADServiceAccount;
				if (aDServiceAccount != null)
				{
					ADServiceAccount[] aDServiceAccountArray1 = new ADServiceAccount[1];
					aDServiceAccountArray1[0] = aDServiceAccount;
					aDServiceAccountArray = aDServiceAccountArray1;
				}
			}
			if (aDServiceAccountArray == null)
			{
				ArgumentException argumentException = new ArgumentException(StringResources.ObjectNotFound);
				base.ThrowTerminatingError(new ErrorRecord(argumentException, "SetADComputerServiceAccount:BeginProcessingOverride", ErrorCategory.ObjectNotFound, this));
			}
			else
			{
				List<string> strs = new List<string>();
				List<ADServiceAccount> aDServiceAccounts = new List<ADServiceAccount>();
				Hashtable hashtables = new Hashtable();
				ADServiceAccountFactory<ADServiceAccount> aDServiceAccountFactory = new ADServiceAccountFactory<ADServiceAccount>();
				ADServiceAccount[] aDServiceAccountArray2 = aDServiceAccountArray;
				for (int i = 0; i < (int)aDServiceAccountArray2.Length; i++)
				{
					ADServiceAccount aDServiceAccount1 = aDServiceAccountArray2[i];
					base.SetPipelinedSessionInfo(aDServiceAccount1.SessionInfo);
					CmdletSessionInfo cmdletSessionInfo = base.GetCmdletSessionInfo();
					aDServiceAccountFactory.SetCmdletSessionInfo(cmdletSessionInfo);
					try
					{
						ADObject directoryObjectFromIdentity = aDServiceAccountFactory.GetDirectoryObjectFromIdentity(aDServiceAccount1, cmdletSessionInfo.DefaultPartitionPath);
						strs.Add(directoryObjectFromIdentity.DistinguishedName);
						this._resolvedServiceAccountList.Add(directoryObjectFromIdentity);
					}
					catch (ADIdentityNotFoundException aDIdentityNotFoundException1)
					{
						ADIdentityNotFoundException aDIdentityNotFoundException = aDIdentityNotFoundException1;
						DebugLogger.LogError("SetADComputerServiceAccount", aDIdentityNotFoundException.ToString());
						aDServiceAccounts.Add(aDServiceAccount1);
					}
				}
				if (aDServiceAccounts.Count > 0)
				{
					ArgumentException argumentException1 = new ArgumentException(StringResources.ObjectNotFound);
					base.ThrowTerminatingError(new ErrorRecord(argumentException1, "SetADComputerServiceAccount:BeginProcessingOverride", ErrorCategory.ObjectNotFound, aDServiceAccounts.ToArray()));
				}
				hashtables.Add(this._modifyOpString, strs.ToArray());
				this._cmdletParameters.RemoveParameter("ServiceAccount");
				this._cmdletParameters["ServiceAccount"] = new ADMultivalueHashtableParameter<string>(hashtables);
			}
			return true;
		}
	}
}