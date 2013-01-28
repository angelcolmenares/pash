using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Disable", "ADOptionalFeature", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219295", SupportsShouldProcess=true, ConfirmImpact=ConfirmImpact.High)]
	public class DisableADOptionalFeature : ModifyADOptionalFeatureBase<DisableADOptionalFeatureParameterSet>
	{
		public DisableADOptionalFeature() : base((ModifyADOptionalFeatureAction)1)
		{
		}
	}
}