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
	internal class CsdlSemanticsDateTimeConstantExpression : CsdlSemanticsExpression, IEdmDateTimeConstantExpression, IEdmExpression, IEdmDateTimeValue, IEdmPrimitiveValue, IEdmValue, IEdmElement, IEdmCheckable
	{
		private readonly CsdlConstantExpression expression;

		private readonly Cache<CsdlSemanticsDateTimeConstantExpression, DateTime> valueCache;

		private readonly static Func<CsdlSemanticsDateTimeConstantExpression, DateTime> ComputeValueFunc;

		private readonly Cache<CsdlSemanticsDateTimeConstantExpression, IEnumerable<EdmError>> errorsCache;

		private readonly static Func<CsdlSemanticsDateTimeConstantExpression, IEnumerable<EdmError>> ComputeErrorsFunc;

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
				return this.errorsCache.GetValue(this, CsdlSemanticsDateTimeConstantExpression.ComputeErrorsFunc, null);
			}
		}

		public override EdmExpressionKind ExpressionKind
		{
			get
			{
				return EdmExpressionKind.DateTimeConstant;
			}
		}

		public IEdmTypeReference Type
		{
			get
			{
				return null;
			}
		}

		public DateTime Value
		{
			get
			{
				return this.valueCache.GetValue(this, CsdlSemanticsDateTimeConstantExpression.ComputeValueFunc, null);
			}
		}

		public EdmValueKind ValueKind
		{
			get
			{
				return this.expression.ValueKind;
			}
		}

		static CsdlSemanticsDateTimeConstantExpression()
		{
			CsdlSemanticsDateTimeConstantExpression.ComputeValueFunc = (CsdlSemanticsDateTimeConstantExpression me) => me.ComputeValue();
			CsdlSemanticsDateTimeConstantExpression.ComputeErrorsFunc = (CsdlSemanticsDateTimeConstantExpression me) => me.ComputeErrors();
		}

		public CsdlSemanticsDateTimeConstantExpression(CsdlConstantExpression expression, CsdlSemanticsSchema schema) : base(schema, expression)
		{
			this.valueCache = new Cache<CsdlSemanticsDateTimeConstantExpression, DateTime>();
			this.errorsCache = new Cache<CsdlSemanticsDateTimeConstantExpression, IEnumerable<EdmError>>();
			this.expression = expression;
		}

		private IEnumerable<EdmError> ComputeErrors()
		{
			DateTime? nullable;
			if (EdmValueParser.TryParseDateTime(this.expression.Value, out nullable))
			{
				return Enumerable.Empty<EdmError>();
			}
			else
			{
				EdmError[] edmError = new EdmError[1];
				edmError[0] = new EdmError(base.Location, EdmErrorCode.InvalidDateTime, Strings.ValueParser_InvalidDateTime(this.expression.Value));
				return edmError;
			}
		}

		private DateTime ComputeValue()
		{
			DateTime? nullable;
			if (EdmValueParser.TryParseDateTime(this.expression.Value, out nullable))
			{
				return nullable.Value;
			}
			else
			{
				return DateTime.MinValue;
			}
		}
	}
}