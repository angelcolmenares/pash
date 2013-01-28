using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Expressions;
using Microsoft.Data.Edm.Values;
using System;

namespace Microsoft.Data.Edm.Library.Values
{
	internal class EdmTimeConstant : EdmValue, IEdmTimeConstantExpression, IEdmExpression, IEdmTimeValue, IEdmPrimitiveValue, IEdmValue, IEdmElement
	{
		private readonly TimeSpan @value;

		public EdmExpressionKind ExpressionKind
		{
			get
			{
				return EdmExpressionKind.TimeConstant;
			}
		}

		public TimeSpan Value
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
				return EdmValueKind.Time;
			}
		}

		public EdmTimeConstant(TimeSpan value) : this(null, value)
		{
		}

		public EdmTimeConstant(IEdmTemporalTypeReference type, TimeSpan value) : base(type)
		{
			this.@value = value;
		}
	}
}