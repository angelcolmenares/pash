using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Expressions;
using Microsoft.Data.Edm.Values;
using System;

namespace Microsoft.Data.Edm.Library.Values
{
	internal class EdmStringConstant : EdmValue, IEdmStringConstantExpression, IEdmExpression, IEdmStringValue, IEdmPrimitiveValue, IEdmValue, IEdmElement
	{
		private readonly string @value;

		public EdmExpressionKind ExpressionKind
		{
			get
			{
				return EdmExpressionKind.StringConstant;
			}
		}

		public string Value
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
				return EdmValueKind.String;
			}
		}

		public EdmStringConstant(string value) : this(null, value)
		{
		}

		public EdmStringConstant(IEdmStringTypeReference type, string value) : base(type)
		{
			EdmUtil.CheckArgumentNull<string>(value, "value");
			this.@value = value;
		}
	}
}