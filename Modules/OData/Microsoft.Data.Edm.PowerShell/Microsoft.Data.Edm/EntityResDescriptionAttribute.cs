using System;
using System.ComponentModel;

namespace Microsoft.Data.Edm
{
	[AttributeUsage(AttributeTargets.All)]
	internal sealed class EntityResDescriptionAttribute : DescriptionAttribute
	{
		private bool replaced;

		public override string Description
		{
			get
			{
				if (!this.replaced)
				{
					this.replaced = true;
					base.DescriptionValue = EntityRes.GetString(base.Description);
				}
				return base.Description;
			}
		}

		public EntityResDescriptionAttribute(string description) : base(description)
		{
		}
	}
}