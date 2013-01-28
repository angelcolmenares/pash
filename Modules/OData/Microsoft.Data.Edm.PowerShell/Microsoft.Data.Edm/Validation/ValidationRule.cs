using System;

namespace Microsoft.Data.Edm.Validation
{
	internal abstract class ValidationRule
	{
		internal abstract Type ValidatedType
		{
			get;
		}

		protected ValidationRule()
		{
		}

		internal abstract void Evaluate(ValidationContext context, object item);
	}
}