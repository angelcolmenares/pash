using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("New", "ADClaimTransformPolicy", HelpUri="http://go.microsoft.com/fwlink/?LinkId=216374", SupportsShouldProcess=true)]
	public class NewADClaimTransformPolicy : ADNewCmdletBase<NewADClaimTransformPolicyParameterSet, ADClaimTransformPolicyFactory<ADClaimTransformPolicy>, ADClaimTransformPolicy>
	{
		public NewADClaimTransformPolicy()
		{
			base.BeginProcessPipeline.InsertAtEnd(new CmdletSubroutine(this.NewADClaimTransformPolicyBeginCSRoutine));
		}

		protected internal override string GetDefaultCreationPathBase()
		{
			return ADPathModule.MakePath(this.GetRootDSE().ConfigurationNamingContext, "CN=Claims Transformation Policies,CN=Claims Configuration,CN=Services,", ADPathFormat.X500);
		}

		private bool NewADClaimTransformPolicyBeginCSRoutine()
		{
			ADCBACUtil.InsertClaimTransformRule(this._cmdletParameters, this.GetCmdletSessionInfo());
			return true;
		}
	}
}