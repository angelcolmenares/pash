using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class ADServiceAccountCmdletBase<P> : ADCmdletBase<P>, IADErrorTarget
	where P : ADParameterSet, new()
	{
		protected ADServiceAccountFactory<ADServiceAccount> _factory;

		private string _cmdletVerb;

		public ADServiceAccountCmdletBase(string cmdletVerb)
		{
			this._factory = new ADServiceAccountFactory<ADServiceAccount>();
			this._cmdletVerb = cmdletVerb;
			base.ProcessRecordPipeline.InsertAtEnd(new CmdletSubroutine(this.ADServiceAccountCmdletBaseProcessCSRoutine));
		}

		private bool ADServiceAccountCmdletBaseProcessCSRoutine()
		{
			ADObject directoryObjectFromIdentity;
			ADServiceAccount item = this._cmdletParameters["Identity"] as ADServiceAccount;
			this.SetPipelinedSessionInfo(item.SessionInfo);
			CmdletSessionInfo cmdletSessionInfo = this.GetCmdletSessionInfo();
			this._factory.SetCmdletSessionInfo(cmdletSessionInfo);
			ICollection<string> extendedPropertiesToFetch = this.GetExtendedPropertiesToFetch();
			if (extendedPropertiesToFetch == null || extendedPropertiesToFetch.Count == 0)
			{
				directoryObjectFromIdentity = this._factory.GetDirectoryObjectFromIdentity(item, cmdletSessionInfo.DefaultPartitionPath);
			}
			else
			{
				directoryObjectFromIdentity = this._factory.GetExtendedObjectFromIdentity(item, cmdletSessionInfo.DefaultPartitionPath, extendedPropertiesToFetch, false);
			}
			if (base.ShouldProcessOverride(directoryObjectFromIdentity.DistinguishedName, this._cmdletVerb))
			{
				this.PerformOperation(directoryObjectFromIdentity);
			}
			return true;
		}

		protected internal override string GetDefaultPartitionPath()
		{
			return this.GetRootDSE().DefaultNamingContext;
		}

		protected internal virtual ICollection<string> GetExtendedPropertiesToFetch()
		{
			return null;
		}

		object Microsoft.ActiveDirectory.Management.Commands.IADErrorTarget.CurrentIdentity(Exception e)
		{
			if (this._cmdletParameters.Contains("Server"))
			{
				return this._cmdletParameters["Server"];
			}
			else
			{
				return null;
			}
		}

		protected internal virtual void PerformOperation(ADObject serviceAccount)
		{
		}

		protected string TrimServiceAccountSamAccountName(string SamAccountName)
		{
			string samAccountName = SamAccountName;
			if (!string.IsNullOrEmpty(samAccountName) && samAccountName.EndsWith("$", StringComparison.OrdinalIgnoreCase))
			{
				samAccountName = samAccountName.Substring(0, samAccountName.Length - 1);
			}
			return samAccountName;
		}

		protected void ValidateServiceAccountSamAccountNameLength(string SamAccountName)
		{
			if (SamAccountName.Length <= 15)
			{
				return;
			}
			else
			{
				object[] item = new object[2];
				item[0] = this._cmdletParameters["Name"];
				item[1] = this._cmdletParameters["SamAccountName"];
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.ServiceAccountNameLengthInvalid, item));
			}
		}
	}
}