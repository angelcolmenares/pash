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
	internal class CsdlSemanticsBooleanConstantExpression : CsdlSemanticsExpression, IEdmBooleanConstantExpression, IEdmExpression, IEdmBooleanValue, IEdmPrimitiveValue, IEdmValue, IEdmElement, IEdmCheckable
	{
		private readonly CsdlConstantExpression expression;

		private readonly Cache<CsdlSemanticsBooleanConstantExpression, bool> valueCache;

		private readonly static Func<CsdlSemanticsBooleanConstantExpression, bool> ComputeValueFunc;

		private readonly Cache<CsdlSemanticsBooleanConstantExpression, IEnumerable<EdmError>> errorsCache;

		private readonly static Func<CsdlSemanticsBooleanConstantExpression, IEnumerable<EdmError>> ComputeErrorsFunc;

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
				return this.errorsCache.GetValue(this, CsdlSemanticsBooleanConstantExpression.ComputeErrorsFunc, null);
			}
		}

		public override EdmExpressionKind ExpressionKind
		{
			get
			{
				return EdmExpressionKind.BooleanConstant;
			}
		}

		public IEdmTypeReference Type
		{
			get
			{
				return null;
			}
		}

		public bool Value
		{
			get
			{
				return this.valueCache.GetValue(this, CsdlSemanticsBooleanConstantExpression.ComputeValueFunc, null);
			}
		}

		public EdmValueKind ValueKind
		{
			get
			{
				return this.expression.ValueKind;
			}
		}

		static CsdlSemanticsBooleanConstantExpression()
		{
			CsdlSemanticsBooleanConstantExpression.ComputeValueFunc = (CsdlSemanticsBooleanConstantExpression me) => me.ComputeValue();
			CsdlSemanticsBooleanConstantExpression.ComputeErrorsFunc = (CsdlSemanticsBooleanConstantExpression me) => me.ComputeErrors();
		}

		public CsdlSemanticsBooleanConstantExpression(CsdlConstantExpression expression, CsdlSemanticsSchema schema) : base(schema, expression)
		{
			this.valueCache = new Cache<CsdlSemanticsBooleanConstantExpression, bool>();
			this.errorsCache = new Cache<CsdlSemanticsBooleanConstantExpression, IEnumerable<EdmError>>();
			this.expression = expression;
		}

		private IEnumerable<EdmError> ComputeErrors()
		{
			bool? nullable;
			if (EdmValueParser.TryParseBool(this.expression.Value, out nullable))
			{
				return Enumerable.Empty<EdmError>();
			}
			else
			{
				EdmError[] edmError = new EdmError[1];
				edmError[0] = new EdmError(base.Location, EdmErrorCode.InvalidBoolean, Strings.ValueParser_InvalidBoolean(this.expression.Value));
				return edmError;
			}
		}

		private bool ComputeValue()
		{
			bool? nullable;
			if (EdmValueParser.TryParseBool(this.expression.Value, out nullable))
			{
				return nullable.Value;
			}
			else
			{
				return false;
			}
		}
	}
}