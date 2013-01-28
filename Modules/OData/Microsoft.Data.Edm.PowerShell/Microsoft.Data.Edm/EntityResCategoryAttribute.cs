using System;
using System.ComponentModel;

namespace Microsoft.Data.Edm
{
	[AttributeUsage(AttributeTargets.All)]
	internal sealed class EntityResCategoryAttribute : CategoryAttribute
	{
		public EntityResCategoryAttribute(string category) : base(category)
		{
		}

		protected override string GetLocalizedString(string value)
		{
			return EntityRes.GetString(value);
		}
	}
}