using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Expressions;
using Microsoft.Data.Edm.Values;
using System;

namespace Microsoft.Data.Edm.Library.Values
{
	internal class EdmDateTimeConstant : EdmValue, IEdmDateTimeConstantExpression, IEdmExpression, IEdmDateTimeValue, IEdmPrimitiveValue, IEdmValue, IEdmElement
	{
		private readonly DateTime @value;

		public EdmExpressionKind ExpressionKind
		{
			get
			{
				return EdmExpressionKind.DateTimeConstant;
			}
		}

		public DateTime Value
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
				return EdmValueKind.DateTime;
			}
		}

		public EdmDateTimeConstant(DateTime value) : this(null, value)
		{
		}

		public EdmDateTimeConstant(IEdmTemporalTypeReference type, DateTime value) : base(type)
		{
			this.@value = value;
		}
	}
}