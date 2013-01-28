using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
	internal sealed class ProviderRemoveItemDynamicParameters
	{
		private SwitchParameter deleteKey;

		[Parameter]
		public SwitchParameter DeleteKey
		{
			get
			{
				return this.deleteKey;
			}
			set
			{
				this.deleteKey = value;
			}
		}

		public ProviderRemoveItemDynamicParameters()
		{
			this.deleteKey = new SwitchParameter();
		}
	}
}