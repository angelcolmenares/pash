using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl.Internal;
using Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast;
using Microsoft.Data.Edm.Expressions;
using Microsoft.Data.Edm.Internal;
using Microsoft.Data.Edm.Validation;
using Microsoft.Data.Edm.Values;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal class CsdlSemanticsDecimalConstantExpression : CsdlSemanticsExpression, IEdmDecimalConstantExpression, IEdmExpression, IEdmDecimalValue, IEdmPrimitiveValue, IEdmValue, IEdmElement, IEdmCheckable
	{
		private readonly CsdlConstantExpression expression;

		private readonly Cache<CsdlSemanticsDecimalConstantExpression, decimal> valueCache;

		private readonly static Func<CsdlSemanticsDecimalConstantExpression, decimal> ComputeValueFunc;

		private readonly Cache<CsdlSemanticsDecimalConstantExpression, IEnumerable<EdmError>> errorsCache;

		private readonly static Func<CsdlSemanticsDecimalConstantExpression, IEnumerable<EdmError>> ComputeErrorsFunc;

		public override CsdlElement Element
		{
			get
			{
				return this.expression;
			}
		}

		public IEnumerable<EdmError> Errors
		{
			get
			{
				return this.errorsCache.GetValue(this, CsdlSemanticsDecimalConstantExpression.ComputeErrorsFunc, null);
			}
		}

		public override EdmExpressionKind ExpressionKind
		{
			get
			{
				return EdmExpressionKind.DecimalConstant;
			}
		}

		public IEdmTypeReference Type
		{
			get
			{
				return null;
			}
		}

		public decimal Value
		{
			get
			{
				return this.valueCache.GetValue(this, CsdlSemanticsDecimalConstantExpression.ComputeValueFunc, null);
			}
		}

		public EdmValueKind ValueKind
		{
			get
			{
				return this.expression.ValueKind;
			}
		}

		static CsdlSemanticsDecimalConstantExpression()
		{
			CsdlSemanticsDecimalConstantExpression.ComputeValueFunc = (CsdlSemanticsDecimalConstantExpression me) => me.ComputeValue();
			CsdlSemanticsDecimalConstantExpression.ComputeErrorsFunc = (CsdlSemanticsDecimalConstantExpression me) => me.ComputeErrors();
		}

		public CsdlSemanticsDecimalConstantExpression(CsdlConstantExpression expression, CsdlSemanticsSchema schema) : base(schema, expression)
		{
			this.valueCache = new Cache<CsdlSemanticsDecimalConstantExpression, decimal>();
			this.errorsCache = new Cache<CsdlSemanticsDecimalConstantExpression, IEnumerable<EdmError>>();
			this.expression = expression;
		}

		private IEnumerable<EdmError> ComputeErrors()
		{
			decimal? nullable;
			if (EdmValueParser.TryParseDecimal(this.expression.Value, out nullable))
			{
				return Enumerable.Empty<EdmError>();
			}
			else
			{
				EdmError[] edmError = new EdmError[1];
				edmError[0] = new EdmError(base.Location, EdmErrorCode.InvalidDecimal, Strings.ValueParser_InvalidDecimal(this.expression.Value));
				return edmError;
			}
		}

		private decimal ComputeValue()
		{
			decimal? nullable;
			if (EdmValueParser.TryParseDecimal(this.expression.Value, out nullable))
			{
				return nullable.Value;
			}
			else
			{
				return new decimal(0);
			}
		}
	}
}