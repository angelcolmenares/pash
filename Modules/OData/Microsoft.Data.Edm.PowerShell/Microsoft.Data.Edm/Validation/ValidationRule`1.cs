using Microsoft.Data.Edm;
using System;

namespace Microsoft.Data.Edm.Validation
{
	internal sealed class ValidationRule<TItem> : ValidationRule
	where TItem : IEdmElement
	{
		private readonly Action<ValidationContext, TItem> validate;

		internal override Type ValidatedType
		{
			get
			{
				return typeof(TItem);
			}
		}

		public ValidationRule(Action<ValidationContext, TItem> validate)
		{
			this.validate = validate;
		}

		internal override void Evaluate(ValidationContext context, object item)
		{
			TItem tItem = (TItem)item;
			this.validate(context, tItem);
		}
	}
}