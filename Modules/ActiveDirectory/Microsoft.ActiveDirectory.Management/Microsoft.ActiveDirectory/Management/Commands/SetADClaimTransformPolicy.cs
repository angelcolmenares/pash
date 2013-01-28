using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Set", "ADClaimTransformPolicy", HelpUri="http://go.microsoft.com/fwlink/?LinkId=216410", SupportsShouldProcess=true)]
	public class SetADClaimTransformPolicy : ADSetCmdletBase<SetADClaimTransformPolicyParameterSet, ADClaimTransformPolicyFactory<ADClaimTransformPolicy>, ADClaimTransformPolicy>
	{
		private const string _debugCategory = "SetADClaimTransformPolicy";

		public SetADClaimTransformPolicy()
		{
			base.BeginProcessPipeline.InsertAtEnd(new CmdletSubroutine(this.SetADClaimTransformPolicyBeginCSRoutine));
		}

		protected internal override string GetDefaultPartitionPath()
		{
			return ADPathModule.MakePath(this.GetRootDSE().ConfigurationNamingContext, "CN=Claims Transformation Policies,CN=Claims Configuration,CN=Services,", ADPathFormat.X500);
		}

		private bool SetADClaimTransformPolicyBeginCSRoutine()
		{
			ADCBACUtil.InsertClaimTransformRule(this._cmdletParameters, this.GetCmdletSessionInfo());
			return true;
		}
	}
}