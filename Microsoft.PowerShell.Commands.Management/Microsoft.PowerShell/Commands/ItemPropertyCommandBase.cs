using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
	public class ItemPropertyCommandBase : CoreCommandWithCredentialsBase
	{
		internal string[] paths;

		[Parameter]
		public override string[] Exclude
		{
			get
			{
				return base.Exclude;
			}
			set
			{
				base.Exclude = value;
			}
		}

		[Parameter]
		public override string Filter
		{
			get
			{
				return base.Filter;
			}
			set
			{
				base.Filter = value;
			}
		}

		[Parameter]
		public override string[] Include
		{
			get
			{
				return base.Include;
			}
			set
			{
				base.Include = value;
			}
		}

		public ItemPropertyCommandBase()
		{
			this.paths = new string[0];
		}
	}
}