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
	internal class CsdlSemanticsGuidConstantExpression : CsdlSemanticsExpression, IEdmGuidConstantExpression, IEdmExpression, IEdmGuidValue, IEdmPrimitiveValue, IEdmValue, IEdmElement, IEdmCheckable
	{
		private readonly CsdlConstantExpression expression;

		private readonly Cache<CsdlSemanticsGuidConstantExpression, Guid> valueCache;

		private readonly static Func<CsdlSemanticsGuidConstantExpression, Guid> ComputeValueFunc;

		private readonly Cache<CsdlSemanticsGuidConstantExpression, IEnumerable<EdmError>> errorsCache;

		private readonly static Func<CsdlSemanticsGuidConstantExpression, IEnumerable<EdmError>> ComputeErrorsFunc;

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
				return this.errorsCache.GetValue(this, CsdlSemanticsGuidConstantExpression.ComputeErrorsFunc, null);
			}
		}

		public override EdmExpressionKind ExpressionKind
		{
			get
			{
				return EdmExpressionKind.GuidConstant;
			}
		}

		public IEdmTypeReference Type
		{
			get
			{
				return null;
			}
		}

		public Guid Value
		{
			get
			{
				return this.valueCache.GetValue(this, CsdlSemanticsGuidConstantExpression.ComputeValueFunc, null);
			}
		}

		public EdmValueKind ValueKind
		{
			get
			{
				return this.expression.ValueKind;
			}
		}

		static CsdlSemanticsGuidConstantExpression()
		{
			CsdlSemanticsGuidConstantExpression.ComputeValueFunc = (CsdlSemanticsGuidConstantExpression me) => me.ComputeValue();
			CsdlSemanticsGuidConstantExpression.ComputeErrorsFunc = (CsdlSemanticsGuidConstantExpression me) => me.ComputeErrors();
		}

		public CsdlSemanticsGuidConstantExpression(CsdlConstantExpression expression, CsdlSemanticsSchema schema) : base(schema, expression)
		{
			this.valueCache = new Cache<CsdlSemanticsGuidConstantExpression, Guid>();
			this.errorsCache = new Cache<CsdlSemanticsGuidConstantExpression, IEnumerable<EdmError>>();
			this.expression = expression;
		}

		private IEnumerable<EdmError> ComputeErrors()
		{
			Guid? nullable;
			if (EdmValueParser.TryParseGuid(this.expression.Value, out nullable))
			{
				return Enumerable.Empty<EdmError>();
			}
			else
			{
				EdmError[] edmError = new EdmError[1];
				edmError[0] = new EdmError(base.Location, EdmErrorCode.InvalidGuid, Strings.ValueParser_InvalidGuid(this.expression.Value));
				return edmError;
			}
		}

		private Guid ComputeValue()
		{
			Guid? nullable;
			if (EdmValueParser.TryParseGuid(this.expression.Value, out nullable))
			{
				return nullable.Value;
			}
			else
			{
				return Guid.Empty;
			}
		}
	}
}