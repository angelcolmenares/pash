using Microsoft.ActiveDirectory.Management;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Set", "ADDomainMode", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219356", SupportsShouldProcess=true, ConfirmImpact=ConfirmImpact.High)]
	public class SetADDomainMode : ADSetCmdletBase<SetADDomainModeParameterSet, ADDomainFactory<ADDomain>, ADDomain>
	{
		public SetADDomainMode()
		{
			base.BeginProcessPipeline.InsertAtStart(new CmdletSubroutine(base.GetADCmdletBaseExternalDelegates().TargetPDCEmulatorCSRoutine));
		}

		protected internal override string GetDefaultPartitionPath()
		{
			return this.GetRootDSE().DefaultNamingContext;
		}

		internal override ADSessionInfo GetSessionInfo()
		{
			return ADDomainUtil.ConstructSessionFromIdentity<SetADDomainModeParameterSet, ADDomain>(this, base.GetSessionInfo(), true);
		}
	}
}