using System.Management.Automation;

namespace Microsoft.WSMan.Management
{
	public class WSManProviderSetItemDynamicParameters
	{
		private SwitchParameter _concatenate;

		[Parameter]
		public SwitchParameter Concatenate
		{
			get
			{
				return this._concatenate;
			}
			set
			{
				this._concatenate = value;
			}
		}

		public WSManProviderSetItemDynamicParameters()
		{
			this._concatenate = false;
		}
	}
}