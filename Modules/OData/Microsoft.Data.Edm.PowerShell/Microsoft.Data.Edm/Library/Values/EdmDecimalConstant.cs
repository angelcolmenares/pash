using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Expressions;
using Microsoft.Data.Edm.Values;
using System;

namespace Microsoft.Data.Edm.Library.Values
{
	internal class EdmDecimalConstant : EdmValue, IEdmDecimalConstantExpression, IEdmExpression, IEdmDecimalValue, IEdmPrimitiveValue, IEdmValue, IEdmElement
	{
		private readonly decimal @value;

		public EdmExpressionKind ExpressionKind
		{
			get
			{
				return EdmExpressionKind.DecimalConstant;
			}
		}

		public decimal Value
		{
			get
			{
				return this.@value;
			}
		}

		public override EdmValueKind ValueKind
		{
			get
			{
				return EdmValueKind.Decimal;
			}
		}

		public EdmDecimalConstant(decimal value) : this(null, value)
		{
		}

		public EdmDecimalConstant(IEdmDecimalTypeReference type, decimal value) : base(type)
		{
			this.@value = value;
		}
	}
}