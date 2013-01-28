using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
	public class PassThroughItemPropertyCommandBase : ItemPropertyCommandBase
	{
		private bool passThrough;

		[Parameter]
		public override SwitchParameter Force
		{
			get
			{
				return base.Force;
			}
			set
			{
				base.Force = value;
			}
		}

		[Parameter]
		public SwitchParameter PassThru
		{
			get
			{
				return this.passThrough;
			}
			set
			{
				this.passThrough = value;
			}
		}

		protected override bool ProviderSupportsShouldProcess
		{
			get
			{
				return base.DoesProviderSupportShouldProcess(this.paths);
			}
		}

		public PassThroughItemPropertyCommandBase()
		{
		}

		internal CmdletProviderContext GetCurrentContext()
		{
			CmdletProviderContext cmdletProviderContext = this.CmdletProviderContext;
			cmdletProviderContext.PassThru = this.PassThru;
			return cmdletProviderContext;
		}
	}
}