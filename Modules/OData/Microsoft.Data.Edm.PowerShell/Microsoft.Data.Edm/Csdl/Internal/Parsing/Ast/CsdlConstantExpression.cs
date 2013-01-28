using Microsoft.Data.Edm.Csdl;
using Microsoft.Data.Edm.Expressions;
using Microsoft.Data.Edm.Values;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
	internal class CsdlConstantExpression : CsdlExpressionBase
	{
		private readonly EdmValueKind kind;

		private readonly string @value;

		public override EdmExpressionKind ExpressionKind
		{
			get
			{
				EdmValueKind edmValueKind = this.kind;
				switch (edmValueKind)
				{
					case EdmValueKind.Binary:
					{
						return EdmExpressionKind.BinaryConstant;
					}
					case EdmValueKind.Boolean:
					{
						return EdmExpressionKind.BooleanConstant;
					}
					case EdmValueKind.Collection:
					case EdmValueKind.Enum:
					case EdmValueKind.Structured:
					{
						return EdmExpressionKind.None;
					}
					case EdmValueKind.DateTimeOffset:
					{
						return EdmExpressionKind.DateTimeOffsetConstant;
					}
					case EdmValueKind.DateTime:
					{
						return EdmExpressionKind.DateTimeConstant;
					}
					case EdmValueKind.Decimal:
					{
						return EdmExpressionKind.DecimalConstant;
					}
					case EdmValueKind.Floating:
					{
						return EdmExpressionKind.FloatingConstant;
					}
					case EdmValueKind.Guid:
					{
						return EdmExpressionKind.GuidConstant;
					}
					case EdmValueKind.Integer:
					{
						return EdmExpressionKind.IntegerConstant;
					}
					case EdmValueKind.Null:
					{
						return EdmExpressionKind.Null;
					}
					case EdmValueKind.String:
					{
						return EdmExpressionKind.StringConstant;
					}
					case EdmValueKind.Time:
					{
						return EdmExpressionKind.TimeConstant;
					}
					default:
					{
						return EdmExpressionKind.None;
					}
				}
			}
		}

		public string Value
		{
			get
			{
				return this.@value;
			}
		}

		public EdmValueKind ValueKind
		{
			get
			{
				return this.kind;
			}
		}

		public CsdlConstantExpression(EdmValueKind kind, string value, CsdlLocation location) : base(location)
		{
			this.kind = kind;
			this.@value = value;
		}
	}
}