using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Get", "ADDomainControllerPasswordReplicationPolicyUsage", HelpUri="http://go.microsoft.com/fwlink/?LinkId=216349", DefaultParameterSetName="RevealedAccounts")]
	public class GetADDCPasswordReplicationPolicyUsage : ADGetPropertiesCmdletBase<GetADDomainControllerPasswordReplicationPolicyUsageParameterSet, ADDomainControllerFactory<ADDomainController>, ADDomainController, ADAccountFactory<ADAccount>, ADAccount>
	{
		private const string _debugCategory = "GetADDCPasswordReplicationPolicyUsage";

		private bool _isAuthenticatedAcctSpecified;

		private bool _isRevealedAcctSpecified;

		private bool _autoRangeRetrievalEnabled;

		private HashSet<string> _printedDNsSet;

		private string _sourceProperty;

		private SourcePropertyType _sourcePropertyType;

		internal override bool AutoRangeRetrieve
		{
			get
			{
				return this._autoRangeRetrievalEnabled;
			}
		}

		internal override IdentityLookupMode IdentityLookupMode
		{
			get
			{
				return IdentityLookupMode.DirectoryMode;
			}
		}

		internal override string SourceProperty
		{
			get
			{
				return this._sourceProperty;
			}
		}

		internal override SourcePropertyType SourcePropertyType
		{
			get
			{
				return this._sourcePropertyType;
			}
		}

		public GetADDCPasswordReplicationPolicyUsage()
		{
			this._autoRangeRetrievalEnabled = true;
			this._printedDNsSet = new HashSet<string>();
			this._sourcePropertyType = SourcePropertyType.IdentityInfo;
			base.BeginProcessPipeline.InsertAtStart(new CmdletSubroutine(this.GetADDCPrpUsageBeginCSRoutine));
			base.BeginProcessPipeline.InsertAtStart(new CmdletSubroutine(base.GetADCmdletBaseExternalDelegates().AddSessionOptionWritableDCRequiredCSRoutine));
			base.ProcessRecordPipeline.InsertAtStart(new CmdletSubroutine(this.GetADDCPrpUsageProcessCSRoutine));
		}

		private static string ExtractDNFromRevealedUserInfo(string revealedUserInfo)
		{
			if (revealedUserInfo != null)
			{
				int num = -1;
				for (int i = 0; i < 3; i++)
				{
					num = revealedUserInfo.IndexOf(':', num + 1);
					if (num == -1)
					{
						break;
					}
				}
				return revealedUserInfo.Substring(num + 1);
			}
			else
			{
				throw new ArgumentNullException("revealedUserInfo");
			}
		}

		protected internal override string ExtractIdentityInfoFromSourcePropertyValue(string sourcePropertyValue, out bool isExtractedIdentityDN)
		{
			isExtractedIdentityDN = true;
			if (!this._isRevealedAcctSpecified)
			{
				return sourcePropertyValue;
			}
			else
			{
				string str = GetADDCPasswordReplicationPolicyUsage.ExtractDNFromRevealedUserInfo(sourcePropertyValue);
				if (this._printedDNsSet.Contains(str))
				{
					return null;
				}
				else
				{
					this._printedDNsSet.Add(str);
					return str;
				}
			}
		}

		private bool GetADDCPrpUsageBeginCSRoutine()
		{
			this._isRevealedAcctSpecified = this._cmdletParameters.GetSwitchParameterBooleanValue("RevealedAccounts");
			this._isAuthenticatedAcctSpecified = this._cmdletParameters.GetSwitchParameterBooleanValue("AuthenticatedAccounts");
			if (base.ParameterSetName == "RevealedAccounts" && !this._cmdletParameters.Contains("RevealedAccounts"))
			{
				this._isRevealedAcctSpecified = true;
			}
			if (this._isRevealedAcctSpecified)
			{
				ADSessionOptions aDSessionOption = new ADSessionOptions();
				aDSessionOption.LocatorFlag = new ADLocatorFlags?(ADLocatorFlags.DirectoryServices6Required);
				base.SetCmdletSessionOptions(aDSessionOption);
			}
			if (!this._isAuthenticatedAcctSpecified)
			{
				if (this._isRevealedAcctSpecified)
				{
					this._sourceProperty = "msDS-RevealedList";
					this._sourcePropertyType = SourcePropertyType.IdentityInfo;
				}
			}
			else
			{
				this._sourceProperty = "msDS-AuthenticatedToAccountlist";
				this._autoRangeRetrievalEnabled = false;
				this._sourcePropertyType = SourcePropertyType.LinkedDN;
			}
			return true;
		}

		private bool GetADDCPrpUsageProcessCSRoutine()
		{
			if (this._cmdletParameters.Contains("Identity"))
			{
				ADDomainController item = this._cmdletParameters["Identity"] as ADDomainController;
				this.SetPipelinedSessionInfo(item.SessionInfo);
				CmdletSessionInfo cmdletSessionInfo = base.GetCmdletSessionInfo();
				if (!this._isRevealedAcctSpecified || cmdletSessionInfo.ADRootDSE.IsWindows2008AndAbove())
				{
					if (!cmdletSessionInfo.ADRootDSE.IsWritable())
					{
						object[] dNSHostName = new object[1];
						dNSHostName[0] = cmdletSessionInfo.ADRootDSE.DNSHostName;
						base.WriteWarning(string.Format(CultureInfo.CurrentCulture, StringResources.WarningPolicyUsageNotAccurateOnRODC, dNSHostName));
					}
				}
				else
				{
					base.WriteError(new ErrorRecord(new ArgumentException(StringResources.ErrorResultantPRPSpecifyWindows2008OrAbove), "1", ErrorCategory.InvalidData, null));
					return false;
				}
			}
			return true;
		}
	}
}