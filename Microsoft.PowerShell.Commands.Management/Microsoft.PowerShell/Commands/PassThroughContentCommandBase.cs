using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
	public class PassThroughContentCommandBase : ContentCommandBase
	{
		private bool passThrough;

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
				return base.DoesProviderSupportShouldProcess(base.Path);
			}
		}

		public PassThroughContentCommandBase()
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