using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Expressions;
using Microsoft.Data.Edm.Values;
using System;

namespace Microsoft.Data.Edm.Library.Values
{
	internal class EdmDateTimeOffsetConstant : EdmValue, IEdmDateTimeOffsetConstantExpression, IEdmExpression, IEdmDateTimeOffsetValue, IEdmPrimitiveValue, IEdmValue, IEdmElement
	{
		private readonly DateTimeOffset @value;

		public EdmExpressionKind ExpressionKind
		{
			get
			{
				return EdmExpressionKind.DateTimeOffsetConstant;
			}
		}

		public DateTimeOffset Value
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
				return EdmValueKind.DateTimeOffset;
			}
		}

		public EdmDateTimeOffsetConstant(DateTimeOffset value) : this(null, value)
		{
			this.@value = value;
		}

		public EdmDateTimeOffsetConstant(IEdmTemporalTypeReference type, DateTimeOffset value) : base(type)
		{
			this.@value = value;
		}
	}
}