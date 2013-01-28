using System;
using System.ComponentModel;

namespace Microsoft.PowerShell.Activities
{
	[AttributeUsage(AttributeTargets.All, AllowMultiple=false, Inherited=true)]
	public sealed class BehaviorCategoryAttribute : CategoryAttribute
	{
		public BehaviorCategoryAttribute() : base("")
		{
		}

		protected override string GetLocalizedString(string value)
		{
			return CategoryAttribute.Behavior.Category;
		}
	}
}