using Microsoft.ActiveDirectory.Management;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Get", "ADDomainControllerPasswordReplicationPolicy", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219311", DefaultParameterSetName="AllowedPRP")]
	public class GetADDCPasswordReplicationPolicy : ADGetPropertiesCmdletBase<GetADDomainControllerPasswordReplicationPolicyParameterSet, ADDomainControllerFactory<ADDomainController>, ADDomainController, ADPrincipalFactory<ADPrincipal>, ADPrincipal>
	{
		private bool _isAllowedSpecified;

		private bool _isDeniedSpecified;

		private string _sourceProperty;

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
				return SourcePropertyType.LinkedDN;
			}
		}

		public GetADDCPasswordReplicationPolicy()
		{
			base.BeginProcessPipeline.InsertAtStart(new CmdletSubroutine(this.GetADDCPrpBeginCSRoutine));
			base.ProcessRecordPipeline.Clear();
			base.ProcessRecordPipeline.InsertAtEnd(new CmdletSubroutine(this.GetADDCPrpProcessCSRoutine));
		}

		private bool GetADDCPrpBeginCSRoutine()
		{
			this._isDeniedSpecified = this._cmdletParameters.GetSwitchParameterBooleanValue("Denied");
			this._isAllowedSpecified = this._cmdletParameters.GetSwitchParameterBooleanValue("Allowed");
			if (base.ParameterSetName == "AllowedPRP" && !this._cmdletParameters.Contains("AllowedPRP"))
			{
				this._isAllowedSpecified = true;
			}
			if (!this._isAllowedSpecified)
			{
				if (this._isDeniedSpecified)
				{
					this._sourceProperty = "msDS-NeverRevealGroup";
				}
			}
			else
			{
				this._sourceProperty = "msDS-RevealOnDemandGroup";
			}
			return true;
		}

		private bool GetADDCPrpProcessCSRoutine()
		{
			if (this._cmdletParameters.Contains("Identity"))
			{
				ADDomainController item = this._cmdletParameters["Identity"] as ADDomainController;
				this.SetPipelinedSessionInfo(item.SessionInfo);
				CmdletSessionInfo cmdletSessionInfo = this.GetCmdletSessionInfo();
				this._factory.SetCmdletSessionInfo(cmdletSessionInfo);
				this._returnObjectFactory.SetCmdletSessionInfo(cmdletSessionInfo);
				string[] sourceProperty = new string[1];
				sourceProperty[0] = this.SourceProperty;
				ADObject directoryObjectFromIdentity = this._factory.GetDirectoryObjectFromIdentity(item, cmdletSessionInfo.DefaultPartitionPath, sourceProperty);
				base.WritePropertiesToOutput(item, directoryObjectFromIdentity);
			}
			return true;
		}
	}
}