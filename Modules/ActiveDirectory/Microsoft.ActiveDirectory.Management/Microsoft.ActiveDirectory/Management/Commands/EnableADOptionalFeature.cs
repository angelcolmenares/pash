using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Enable", "ADOptionalFeature", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219297", SupportsShouldProcess=true, ConfirmImpact=ConfirmImpact.High)]
	public class EnableADOptionalFeature : ModifyADOptionalFeatureBase<EnableADOptionalFeatureParameterSet>
	{
		public EnableADOptionalFeature() : base(0)
		{
		}
	}
}