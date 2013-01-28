using System;
using System.ComponentModel;

namespace Microsoft.PowerShell.Activities
{
	[AttributeUsage(AttributeTargets.All, AllowMultiple=false, Inherited=true)]
	public sealed class ParameterSpecificCategoryAttribute : CategoryAttribute
	{
		public ParameterSpecificCategoryAttribute() : base("")
		{
		}

		protected override string GetLocalizedString(string value)
		{
			return Resources.ActivityParameterGroup;
		}
	}
}