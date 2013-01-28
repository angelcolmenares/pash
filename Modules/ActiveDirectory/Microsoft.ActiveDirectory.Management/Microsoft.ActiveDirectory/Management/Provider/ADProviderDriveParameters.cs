using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Provider
{
	internal class ADProviderDriveParameters : ADProviderCommonParameters
	{
		[Parameter]
		public override SwitchParameter GlobalCatalog
		{
			get
			{
				return base.GlobalCatalog;
			}
			set
			{
				base.GlobalCatalog = value;
			}
		}

		public ADProviderDriveParameters()
		{
		}
	}
}