using System;
using System.ComponentModel;

namespace Microsoft.PowerShell.Activities
{
	[AttributeUsage(AttributeTargets.All, AllowMultiple=false, Inherited=true)]
	public sealed class ConnectivityCategoryAttribute : CategoryAttribute
	{
		public ConnectivityCategoryAttribute() : base("")
		{
		}

		protected override string GetLocalizedString(string value)
		{
			return Resources.ConnectivityGroup;
		}
	}
}