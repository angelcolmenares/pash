using System;

namespace Microsoft.PowerShell.Activities
{
	public abstract class GenericCimCmdletActivity : PSGeneratedCIMActivity
	{
		protected override string ModuleDefinition
		{
			get
			{
				return string.Empty;
			}
		}

		public abstract Type TypeImplementingCmdlet
		{
			get;
		}

		protected GenericCimCmdletActivity()
		{
		}
	}
}