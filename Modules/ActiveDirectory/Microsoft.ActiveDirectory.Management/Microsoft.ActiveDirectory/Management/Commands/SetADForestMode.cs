using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using System;
using System.Globalization;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Set", "ADForestMode", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219357", SupportsShouldProcess=true, ConfirmImpact=ConfirmImpact.High)]
	public class SetADForestMode : ADSetCmdletBase<SetADForestModeParameterSet, ADForestFactory<ADForest>, ADForest>
	{
		public SetADForestMode()
		{
			base.ProcessRecordPipeline.InsertAtStart(new CmdletSubroutine(base.GetADCmdletBaseExternalDelegates().TargetSchemaMasterCSRoutine));
		}

		protected internal override string GetDefaultPartitionPath()
		{
			return this.GetRootDSE().RootDomainNamingContext;
		}

		internal override ADSessionInfo GetSessionInfo()
		{
			ADSessionInfo aDSessionInfo;
			try
			{
				aDSessionInfo = ADDomainUtil.ConstructSessionFromIdentity<SetADForestModeParameterSet, ADForest>(this, base.GetSessionInfo(), false);
			}
			catch (ArgumentException argumentException1)
			{
				ArgumentException argumentException = argumentException1;
				object[] item = new object[1];
				item[0] = argumentException.Data["IdentityData"];
				throw new ADIdentityNotFoundException(string.Format(CultureInfo.CurrentCulture, StringResources.CouldNotFindForestIdentity, item));
			}
			return aDSessionInfo;
		}
	}
}