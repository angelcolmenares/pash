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
	internal class CsdlSemanticsTimeConstantExpression : CsdlSemanticsExpression, IEdmTimeConstantExpression, IEdmExpression, IEdmTimeValue, IEdmPrimitiveValue, IEdmValue, IEdmElement, IEdmCheckable
	{
		private readonly CsdlConstantExpression expression;

		private readonly Cache<CsdlSemanticsTimeConstantExpression, TimeSpan> valueCache;

		private readonly static Func<CsdlSemanticsTimeConstantExpression, TimeSpan> ComputeValueFunc;

		private readonly Cache<CsdlSemanticsTimeConstantExpression, IEnumerable<EdmError>> errorsCache;

		private readonly static Func<CsdlSemanticsTimeConstantExpression, IEnumerable<EdmError>> ComputeErrorsFunc;

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
				return this.errorsCache.GetValue(this, CsdlSemanticsTimeConstantExpression.ComputeErrorsFunc, null);
			}
		}

		public override EdmExpressionKind ExpressionKind
		{
			get
			{
				return EdmExpressionKind.TimeConstant;
			}
		}

		public IEdmTypeReference Type
		{
			get
			{
				return null;
			}
		}

		public TimeSpan Value
		{
			get
			{
				return this.valueCache.GetValue(this, CsdlSemanticsTimeConstantExpression.ComputeValueFunc, null);
			}
		}

		public EdmValueKind ValueKind
		{
			get
			{
				return this.expression.ValueKind;
			}
		}

		static CsdlSemanticsTimeConstantExpression()
		{
			CsdlSemanticsTimeConstantExpression.ComputeValueFunc = (CsdlSemanticsTimeConstantExpression me) => me.ComputeValue();
			CsdlSemanticsTimeConstantExpression.ComputeErrorsFunc = (CsdlSemanticsTimeConstantExpression me) => me.ComputeErrors();
		}

		public CsdlSemanticsTimeConstantExpression(CsdlConstantExpression expression, CsdlSemanticsSchema schema) : base(schema, expression)
		{
			this.valueCache = new Cache<CsdlSemanticsTimeConstantExpression, TimeSpan>();
			this.errorsCache = new Cache<CsdlSemanticsTimeConstantExpression, IEnumerable<EdmError>>();
			this.expression = expression;
		}

		private IEnumerable<EdmError> ComputeErrors()
		{
			TimeSpan? nullable;
			if (EdmValueParser.TryParseTime(this.expression.Value, out nullable))
			{
				return Enumerable.Empty<EdmError>();
			}
			else
			{
				EdmError[] edmError = new EdmError[1];
				edmError[0] = new EdmError(base.Location, EdmErrorCode.InvalidTime, Strings.ValueParser_InvalidTime(this.expression.Value));
				return edmError;
			}
		}

		private TimeSpan ComputeValue()
		{
			TimeSpan? nullable;
			if (EdmValueParser.TryParseTime(this.expression.Value, out nullable))
			{
				return nullable.Value;
			}
			else
			{
				return TimeSpan.Zero;
			}
		}
	}
}